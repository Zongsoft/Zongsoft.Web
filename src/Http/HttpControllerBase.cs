using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

using Zongsoft.Data;
using Zongsoft.Common;
using Zongsoft.Services;

namespace Zongsoft.Web.Http
{
	public class HttpControllerBase<TEntity> : System.Web.Http.ApiController where TEntity : class
	{
		#region 成员字段
		private IDataService<TEntity> _dataService;
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
		protected virtual IDataService<TEntity> DataService
		{
			get
			{
				if(_dataService == null)
					_dataService = _serviceProvider.EnsureResolve<IDataService<TEntity>>();

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
		[Zongsoft.Web.Http.HttpPaging]
		public virtual object Get(string id = null, [FromUri]Paging paging = null)
		{
			if(string.IsNullOrWhiteSpace(id))
				return this.DataService.Select(null, paging);

			var parts = id.Split('-');

			switch(parts.Length)
			{
				case 1:
					return this.DataService.Get<string>(parts[0], paging);
				case 2:
					return this.DataService.Get<string, string>(parts[0], parts[1], paging);
				case 3:
					return this.DataService.Get<string, string, string>(parts[0], parts[1], parts[2], paging);
				default:
					throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
			}
		}

		public virtual void Delete(string id)
		{
			if(string.IsNullOrWhiteSpace(id))
				return;

			var parts = id.Split('-');
			var succeed = false;

			switch(parts.Length)
			{
				case 1:
					succeed = this.DataService.Delete(parts[0]) > 0;
					break;
				case 2:
					succeed = this.DataService.Delete(parts[0], parts[1]) > 0;
					break;
				case 3:
					succeed = this.DataService.Delete(parts[0], parts[1], parts[2]) > 0;
					break;
				default:
					throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
			}

			if(!succeed)
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}

		public virtual void Put(TEntity model)
		{
			if(model == null)
				throw new ArgumentNullException("model");

			if(this.DataService.Update(model) < 1)
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}

		public virtual TEntity Post(TEntity model)
		{
			if(model == null)
				throw new ArgumentNullException("model");

			if(this.DataService.Insert(model) > 0)
				return model;

			throw new HttpResponseException(System.Net.HttpStatusCode.Conflict);
		}
		#endregion
	}
}