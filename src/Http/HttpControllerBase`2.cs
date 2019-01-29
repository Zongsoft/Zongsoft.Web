/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2016-2018 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Web.
 *
 * Zongsoft.Web is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Web is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Web; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;

using Zongsoft.Data;

namespace Zongsoft.Web.Http
{
	public class HttpControllerBase<TModel, TService> : System.Web.Http.ApiController where TService : class, IDataService<TModel>
	{
		#region 成员字段
		private TService _dataService;
		private Zongsoft.Services.IServiceProvider _serviceProvider;
		#endregion

		#region 构造函数
		protected HttpControllerBase(Zongsoft.Services.IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}
		#endregion

		#region 属性定义
		protected virtual bool CanDelete
		{
			get => this.DataService.CanDelete;
		}

		protected virtual bool CanCreate
		{
			get => this.DataService.CanInsert | this.DataService.CanUpsert;
		}

		protected virtual bool CanUpdate
		{
			get => this.DataService.CanUpdate | this.DataService.CanUpsert;
		}

		protected virtual Zongsoft.Security.Credential Credential
		{
			get
			{
				return (this.RequestContext?.Principal?.Identity as Zongsoft.Security.CredentialIdentity)?.Credential;
			}
		}

		protected TService DataService
		{
			get
			{
				if(_dataService == null)
					_dataService = this.GetService() ?? throw new InvalidOperationException("Missing required data service.");

				return _dataService;
			}
		}

		protected Zongsoft.Services.IServiceProvider ServiceProvider
		{
			get
			{
				return _serviceProvider;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_serviceProvider = value;
			}
		}
		#endregion

		#region 公共方法
		[HttpGet]
		public virtual object Count(string id = null)
		{
			if(string.IsNullOrEmpty(id))
				return this.DataService.Count(null);

			var parts = this.Slice(id);

			//switch(parts.Length)
			//{
			//	case 1:
			//		return this.DataService.Count(parts[0]);
			//	case 2:
			//		return this.DataService.Count(parts[0], parts[1]);
			//	case 3:
			//		return this.DataService.Count(parts[0], parts[1], parts[2]);
			//	default:
			//		return this.BadRequest();
			//}

			return this.BadRequest();
		}

		[HttpGet]
		public virtual object Exists(string id = null)
		{
			var existed = false;

			if(string.IsNullOrEmpty(id))
			{
				existed = this.DataService.Exists(null);
			}
			else
			{
				var parts = this.Slice(id);

				switch(parts.Length)
				{
					case 1:
						existed = this.DataService.Exists(parts[0]);
						break;
					case 2:
						existed = this.DataService.Exists(parts[0], parts[1]);
						break;
					case 3:
						existed = this.DataService.Exists(parts[0], parts[1], parts[2]);
						break;
					default:
						return this.BadRequest();
				}
			}

			if(existed)
				return this.Ok();
			else
				return this.NotFound();
		}

		public virtual object Get(string id = null, [FromUri]string key = null, [FromUri]Paging paging = null)
		{
			if(string.IsNullOrWhiteSpace(id))
			{
				if(string.IsNullOrWhiteSpace(key))
					return this.GetResult(this.DataService.Select(null, this.GetSchema(), paging));
				else
					return this.GetResult(this.DataService.Search(key, this.GetSchema(), paging));
			}

			//如果同时指定了id和key参数，则抛出无效的请求异常(即400错误)
			if(key != null)
				return this.BadRequest("Cannot specify the 'id' and 'key' argument at the same time.");

			var parts = this.Slice(id);
			IPaginator paginator = null;

			switch(parts.Length)
			{
				case 1:
					if(parts[0].Contains(":"))
						return this.GetResult(this.DataService.Search(parts[0], this.GetSchema(), paging));
					else
						return this.GetResult(this.DataService.Get<string>(parts[0], this.GetSchema(), paging, null, out paginator), paginator);
				case 2:
					return this.GetResult(this.DataService.Get<string, string>(parts[0], parts[1], this.GetSchema(), paging, null, out paginator), paginator);
				case 3:
					return this.GetResult(this.DataService.Get<string, string, string>(parts[0], parts[1], parts[2], this.GetSchema(), paging, null, out paginator), paginator);
				default:
					return this.BadRequest();
			}
		}

		public virtual void Delete(string id)
		{
			if(!this.CanDelete)
				throw new HttpResponseException(System.Net.HttpStatusCode.MethodNotAllowed);

			if(string.IsNullOrWhiteSpace(id))
				throw HttpResponseExceptionUtility.BadRequest("Missing the id argument.");

			string[] parts;
			var entries = id.Split('|', ',');

			if(entries != null && entries.Length > 1)
			{
				int count = 0;

				using(var transaction = new Zongsoft.Transactions.Transaction())
				{
					foreach(var entry in entries)
					{
						parts = entry.Split('-');

						switch(parts.Length)
						{
							case 1:
								count += this.DataService.Delete<string>(parts[0]);
								break;
							case 2:
								count += this.DataService.Delete<string, string>(parts[0], parts[1]);
								break;
							case 3:
								count += this.DataService.Delete<string, string, string>(parts[0], parts[1], parts[2]);
								break;
							default:
								throw HttpResponseExceptionUtility.BadRequest("The parts of id argument too many.");
						}
					}

					transaction.Commit();
				}

				return;
			}

			parts = id.Split('-');
			var succeed = false;

			switch(parts.Length)
			{
				case 1:
					succeed = this.DataService.Delete<string>(parts[0]) > 0;
					break;
				case 2:
					succeed = this.DataService.Delete<string, string>(parts[0], parts[1]) > 0;
					break;
				case 3:
					succeed = this.DataService.Delete<string, string, string>(parts[0], parts[1], parts[2]) > 0;
					break;
				default:
					throw HttpResponseExceptionUtility.BadRequest("The parts of id argument too many.");
			}

			if(!succeed)
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}

		public virtual TModel Post(TModel model)
		{
			if(!this.CanCreate)
				throw new HttpResponseException(System.Net.HttpStatusCode.MethodNotAllowed);

			if(model == null || (!this.ModelState.IsValid))
				throw HttpResponseExceptionUtility.BadRequest(this.ModelState);

			if(this.DataService.Insert(model, this.GetSchema()) > 0)
				return model;

			throw new HttpResponseException(System.Net.HttpStatusCode.Conflict);
		}

		public virtual void Put(TModel model)
		{
			if(!this.CanUpdate)
				throw new HttpResponseException(System.Net.HttpStatusCode.MethodNotAllowed);

			if(model == null || (!this.ModelState.IsValid))
				throw HttpResponseExceptionUtility.BadRequest(this.ModelState);

			if(this.DataService.Update(model, this.GetSchema()) < 1)
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}

		[HttpPatch, HttpPut]
		public virtual void Patch(string id, IDictionary<string, object> data)
		{
			if(!this.CanUpdate)
				throw new HttpResponseException(System.Net.HttpStatusCode.MethodNotAllowed);

			if(string.IsNullOrWhiteSpace(id) || data == null)
				throw HttpResponseExceptionUtility.BadRequest("Missing the id argument.");

			var count = 0;
			var parts = id.Split('-');

			switch(parts.Length)
			{
				case 1:
					count = this.DataService.Update<string>(data, parts[0], this.GetSchema());
					break;
				case 2:
					count = this.DataService.Update<string, string>(data, parts[0], parts[1], this.GetSchema());
					break;
				case 3:
					count = this.DataService.Update<string, string, string>(data, parts[0], parts[1], parts[2], this.GetSchema());
					break;
				default:
					throw HttpResponseExceptionUtility.BadRequest("The parts of id argument too many.");
			}

			if(count < 1)
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}
		#endregion

		#region 保护方法
		protected virtual TService GetService()
		{
			return _serviceProvider.ResolveRequired<TService>();
		}

		protected string GetSchema()
		{
			const string HTTP_SCHEMA_HEADER = "x-data-schema";

			IEnumerable<string> values;

			if(this.Request.Headers.TryGetValues(HTTP_SCHEMA_HEADER, out values) && values != null)
				return string.Join(",", values);

			return null;
		}

		protected string[] Slice(string text)
		{
			return Utility.Slice(text);
		}

		protected object GetResult(object data, IPaginator paginator = null)
		{
			if(data == null)
				return new HttpResponseMessage(System.Net.HttpStatusCode.NoContent);

			if(paginator == null)
				paginator = data as IPaginator;

			if(paginator != null)
			{
				var result = new Result(data);

				paginator.Paginated += Paginator_Paginated;

				void Paginator_Paginated(object sender, PagingEventArgs e)
				{
					if(result.Paging == null)
						result.Paging = new Dictionary<string, string>();

					result.Paging[string.IsNullOrEmpty(e.Name) ? "$" : e.Name] = e.Paging.ToString();
				}

				return result;
			}

			return data;
		}
		#endregion

		#region 私有方法
		private object GetResult(object value, out bool isNullOrEmpty)
		{
			isNullOrEmpty = value == null;

			if(value == null)
				return null;

			if(value is IEnumerable)
			{
				var result = new ResultEnumerable((IEnumerable)value);
				isNullOrEmpty = result.IsNullOrEmpty;
				return result;
			}

			return value;
		}
		#endregion

		#region 嵌套子类
		private class Result
		{
			[Zongsoft.Runtime.Serialization.SerializationMember("$")]
			public object Data;

			[Zongsoft.Runtime.Serialization.SerializationMember("$paging")]
			public IDictionary<string, string> Paging;

			public Result(object data)
			{
				this.Data = data;
				this.Paging = null;
			}
		}

		private class ResultEnumerable : IEnumerable
		{
			#region 成员字段
			private ResultEnumerator _enumerator;
			#endregion

			#region 构造函数
			public ResultEnumerable(IEnumerable items)
			{
				_enumerator = new ResultEnumerator(items);
			}
			#endregion

			#region 公共属性
			public bool IsNullOrEmpty
			{
				get
				{
					return _enumerator.IsNullOrEmpty;
				}
			}
			#endregion

			#region 公共方法
			public IEnumerator GetEnumerator()
			{
				return _enumerator;
			}
			#endregion
		}

		private class ResultEnumerator : IEnumerator
		{
			#region 私有变量
			private int _flag;
			private bool _isNullOrEmpty;
			private IEnumerator _iterator;
			#endregion

			#region 构造函数
			public ResultEnumerator(IEnumerable items)
			{
				_iterator = items.GetEnumerator();
				_isNullOrEmpty = !_iterator.MoveNext();
			}
			#endregion

			#region 公共属性
			public bool IsNullOrEmpty
			{
				get
				{
					return _isNullOrEmpty;
				}
			}

			public object Current
			{
				get
				{
					return _iterator.Current;
				}
			}
			#endregion

			#region 公共方法
			public bool MoveNext()
			{
				if(System.Threading.Interlocked.Exchange(ref _flag, 1) == 0)
					return !_isNullOrEmpty;

				return _iterator.MoveNext();
			}

			public void Reset()
			{
				_iterator.Reset();
			}
			#endregion
		}
		#endregion
	}
}