/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2016 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Web.Http;

using Zongsoft.Data;

namespace Zongsoft.Web.Http
{
	public class HttpControllerBase<TModel, TConditional, TService> : System.Web.Http.ApiController
																	  where TModel : class
																	  where TConditional : class, IConditional
																	  where TService : class, IDataService<TModel>
	{
		#region 成员字段
		private TService _dataService;
		private Zongsoft.Services.IServiceProvider _serviceProvider;
		#endregion

		#region 构造函数
		protected HttpControllerBase(Zongsoft.Services.IServiceProvider serviceProvider)
		{
			if(serviceProvider == null)
				throw new ArgumentNullException("serviceProvider");

			_serviceProvider = serviceProvider;
		}
		#endregion

		#region 属性定义
		protected Zongsoft.Security.Credential Credential
		{
			get
			{
				return (this.RequestContext?.Principal?.Identity as Zongsoft.Security.CredentialIdentity)?.Credential;
			}
		}

		[Zongsoft.Services.ServiceDependency]
		protected TService DataService
		{
			get
			{
				return _dataService;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_dataService = value;
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
		[HttpPaging]
		public virtual object Get(string id = null, [FromUri]string key = null, [FromUri]Paging paging = null)
		{
			object result = null;

			if(string.IsNullOrWhiteSpace(id))
			{
				if(string.IsNullOrWhiteSpace(key))
					return this.DataService.Select(null, paging);

				result = this.DataService.Search(key, paging);

				if(result == null)
					throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);

				return result;
			}

			//如果同时指定了id和key参数，则抛出无效的请求异常(即400错误)
			if(key != null)
				throw HttpResponseExceptionUtility.BadRequest("Cannot specify the 'id' and 'key' argument at the same time.");

			var parts = this.Slice(id);

			switch(parts.Length)
			{
				case 1:
					//如果只指定了一个参数并且参数包含冒号，则直接返回搜索操作结果
					if(parts[0].Contains(":"))
					{
						result = this.DataService.Search(parts[0], paging);

						if(result == null)
							throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);

						return result;
					}

					result = this.DataService.Get<string>(parts[0], paging);
					break;
				case 2:
					result = this.DataService.Get<string, string>(parts[0], parts[1], paging);
					break;
				case 3:
					result = this.DataService.Get<string, string, string>(parts[0], parts[1], parts[2], paging);
					break;
				default:
					throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
			}

			//如果结果对象为空则抛出404异常
			if(result == null)
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);

			return result;
		}

		[HttpGet]
		[HttpPaging]
		public virtual object Search(string keyword, [FromUri]Paging paging = null)
		{
			if(string.IsNullOrWhiteSpace(keyword))
				throw HttpResponseExceptionUtility.BadRequest("Missing keyword for search.");

			return this.DataService.Search(keyword, paging);
		}

		public virtual void Delete(string id)
		{
			if(string.IsNullOrWhiteSpace(id))
				throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);

			var parts = id.Split('-');
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
					throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
			}

			if(!succeed)
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}

		public virtual void Put(TModel model)
		{
			if(model == null || (!this.ModelState.IsValid))
				throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);

			if(this.DataService.Update(model) < 1)
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}

		public virtual TModel Post(TModel model)
		{
			if(model == null || (!this.ModelState.IsValid))
				throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);

			if(this.DataService.Insert(model) > 0)
				return model;

			throw new HttpResponseException(System.Net.HttpStatusCode.Conflict);
		}

		[HttpPatch, HttpPut]
		public virtual void Patch(string id, [FromContent]IDictionary<string, object> data)
		{
			if(string.IsNullOrWhiteSpace(id) || data == null)
				throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);

			var count = 0;
			var parts = id.Split('-');

			switch(parts.Length)
			{
				case 1:
					count = this.DataService.Update<string>(data, parts[0]);
					break;
				case 2:
					count = this.DataService.Update<string, string>(data, parts[0], parts[1]);
					break;
				case 3:
					count = this.DataService.Update<string, string, string>(data, parts[0], parts[1], parts[2]);
					break;
				default:
					throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
			}

			if(count < 1)
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}

		[HttpPost]
		[HttpPaging]
		public virtual IEnumerable<TModel> Query(TConditional conditional, [FromUri]Paging paging = null)
		{
			return this.DataService.Select(conditional, paging);
		}
		#endregion

		#region 保护方法
		protected string[] Slice(string text)
		{
			return Utility.Slice(text);
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