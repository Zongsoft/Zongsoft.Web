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
using System.Collections.Generic;
using System.Web.Mvc;

using Zongsoft.Data;
using Zongsoft.Security;

namespace Zongsoft.Web
{
	public class ControllerBase<TModel, TConditional, TService> : System.Web.Mvc.Controller
																  where TModel : class
																  where TConditional : class, IConditional
																  where TService : class, IDataService<TModel>
	{
		#region 成员字段
		private TService _dataService;
		private Zongsoft.Services.IServiceProvider _serviceProvider;
		#endregion

		#region 构造函数
		protected ControllerBase(Zongsoft.Services.IServiceProvider serviceProvider)
		{
			if(serviceProvider == null)
				throw new ArgumentNullException("serviceProvider");

			_serviceProvider = serviceProvider;
		}
		#endregion

		#region 属性定义
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

		protected virtual Credential Credential
		{
			get
			{
				var principal = base.User as Zongsoft.Security.CredentialPrincipal;

				if(principal == null || principal.Identity == null || principal.Identity.Credential == null)
					return null;

				return principal.Identity.Credential;
			}
		}
		#endregion

		#region 公共方法
		[HttpGet]
		public virtual ActionResult Index(string id = null, Paging paging = null)
		{
			//将分页信息传递给视图
			this.ViewData["Paging"] = paging;

			//根据当前请求获取数据
			object data = string.IsNullOrWhiteSpace(id) ? this.OnIndex((TConditional)null, null, paging) : this.OnIndex(id, null, paging);

			//如果模型数据为集合类型，则返回“Index”默认视图；否则返回“Details”详细视图
			if(data != null && (data is System.Collections.IEnumerable || Zongsoft.Common.TypeExtension.IsAssignableFrom(typeof(IEnumerable<>), data.GetType())))
				return this.View(this.GetViewModel("Index", data));
			else
				return this.View("Details", this.GetViewModel("Index", data));
		}

		[HttpPost]
		public virtual ActionResult Index(TConditional conditional, Paging paging = null)
		{
			//将分页信息传递给视图
			this.ViewData["Paging"] = paging;
			//将查询条件传递给视图
			this.ViewData["Conditional"] = conditional;

			//获取视图模型
			var model = this.GetViewModel("Index", this.OnIndex(conditional, null, paging));

			return this.View(model);
		}

		[HttpGet]
		public virtual ActionResult Edit(string id)
		{
			if(string.IsNullOrWhiteSpace(id))
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

			//获取视图模型
			var model = this.GetViewModel("Edit", this.OnIndex(id));

			return this.View(model);
		}

		[HttpPost]
		public virtual ActionResult Edit(TModel model, string redirectUrl = null)
		{
			if(model == null)
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

			if(!this.ModelState.IsValid)
				return this.View(this.GetViewModel("Edit", model));

			try
			{
				if(this.OnEdit(model) > 0)
				{
					if(!string.IsNullOrWhiteSpace(redirectUrl))
						return this.Redirect(redirectUrl);
				}
				else
				{
					this.ModelState.AddModelError(string.Empty, Resources.ResourceUtility.GetString("Text.DataUpdateFailed"));
				}
			}
			catch(Exception ex)
			{
				this.ModelState.AddModelError(string.Empty, ex.Message);
			}

			return this.View(this.GetViewModel("Edit", model));
		}

		[HttpGet]
		public virtual ActionResult Create()
		{
			return this.View(this.GetViewModel("Create", null));
		}

		[HttpPost]
		public virtual ActionResult Create(TModel model, string redirectUrl = null)
		{
			if(model == null)
				return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

			if(!this.ModelState.IsValid)
				return this.View(this.GetViewModel("Create", model));

			try
			{
				if(this.OnCreate(model) > 0)
				{
					if(!string.IsNullOrWhiteSpace(redirectUrl))
						return this.Redirect(redirectUrl);
				}
				else
				{
					this.ModelState.AddModelError(string.Empty, Resources.ResourceUtility.GetString("Text.DataCreateFailed"));
				}
			}
			catch(Exception ex)
			{
				this.ModelState.AddModelError(string.Empty, ex.Message);
			}

			return this.View(this.GetViewModel("Create", model));
		}

		[HttpGet]
		public virtual ActionResult Delete(string id)
		{
			return this.View(this.GetViewModel("Delete", this.OnIndex(id)));
		}

		[HttpDelete]
		public virtual ActionResult Delete(string id, string redirectUrl = null)
		{
			if(string.IsNullOrWhiteSpace(id))
				return this.View();

			if(this.OnDelete(id) < 1)
			{
				this.ModelState.AddModelError(string.Empty, Resources.ResourceUtility.GetString("Text.DataDeleteFailed"));
				return this.View();
			}

			if(string.IsNullOrWhiteSpace(redirectUrl))
				return this.View("Index");
			else
				return this.Redirect(redirectUrl);
		}
		#endregion

		#region 保护方法
		protected virtual object OnIndex(string id, string scope = null, Paging paging = null, params Sorting[] sortings)
		{
			if(string.IsNullOrWhiteSpace(id))
				return null;

			var parts = Utility.Slice(id);

			switch(parts.Length)
			{
				case 1:
					return this.DataService.Get<string>(parts[0], scope, paging, sortings);
				case 2:
					return this.DataService.Get<string, string>(parts[0], parts[1], scope, paging, sortings);
				case 3:
					return this.DataService.Get<string, string, string>(parts[0], parts[1], parts[2], scope, paging, sortings);
				default:
					return null;
			}
		}

		protected virtual object OnIndex(TConditional conditional, string scope = null, Paging paging = null, params Sorting[] sortings)
		{
			return this.DataService.Select(conditional.ToCondition(), scope, paging, sortings);
		}

		protected virtual int OnDelete(string id, string cascades = null)
		{
			if(string.IsNullOrWhiteSpace(id))
				return 0;

			var parts = id.Split('-');

			switch(parts.Length)
			{
				case 1:
					return this.DataService.Delete<string>(parts[0], cascades);
				case 2:
					return this.DataService.Delete<string, string>(parts[0], parts[1], cascades);
				case 3:
					return this.DataService.Delete<string, string, string>(parts[0], parts[1], parts[2], cascades);
			}

			return -1;
		}

		protected virtual int OnEdit(TModel model, string scope = null, ICondition condition = null)
		{
			return this.DataService.Update(model, condition, scope);
		}

		protected virtual int OnCreate(TModel model, string scope = null)
		{
			return this.DataService.Insert(model, scope);
		}

		protected virtual object GetViewModel(string actionName, object model)
		{
			if(model == null && string.Equals(actionName, "Create", StringComparison.OrdinalIgnoreCase))
				return Activator.CreateInstance<TModel>();

			return model;
		}
		#endregion
	}
}