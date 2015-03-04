/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2011-2013 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

using Zongsoft.Data;
using Zongsoft.Data.Entities;
using Zongsoft.ComponentModel;
using Zongsoft.Services;
using Zongsoft.Security;
using Zongsoft.Security.Membership;

namespace Zongsoft.Web.Mvc
{
	[Obsolete]
	public class DataControllerBase<TCondition, TModel> : AuthorizationController where TCondition : class, ICondition
	{
		#region 成员变量
		private string _name;
		private string _conditionKey;
		private IObjectAccess _objectAccess;
		private IServiceProviderFactory _serviceProviderFactory;
		#endregion

		#region 构造函数
		public DataControllerBase(IServiceProviderFactory serviceProviderFactory):base(serviceProviderFactory)
		{
			if(serviceProviderFactory == null)
				throw new ArgumentNullException("serviceProviderFactory");

			_name = typeof(TModel).Name;
			_conditionKey = this.GetType().FullName;
			_serviceProviderFactory = serviceProviderFactory;
		}
		#endregion

		#region 公共属性
		public virtual string Name
		{
			get
			{
				return _name;
			}
			protected set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();

				_name = value.Trim();
			}
		}

		public TCondition Condition
		{
			get
			{
				TCondition condition = this.TempData[_conditionKey] as TCondition;

				if(condition == null)
				{
					condition = this.CreateCondition();
					this.TempData[_conditionKey] = condition;
				}
				else
				{
					this.TempData.Keep(_conditionKey);
				}

				return condition;
			}
		}

		public IObjectAccess ObjectAccess
		{
			get
			{
				if(_objectAccess == null)
					System.Threading.Interlocked.CompareExchange(ref _objectAccess, this.CreateObjectAccess(), null);

				return _objectAccess;
			}
			protected set
			{
				_objectAccess = value;
			}
		}

		public IServiceProviderFactory ServiceProviderFactory
		{
			get
			{
				return _serviceProviderFactory;
			}
		}
		#endregion

		#region 虚拟方法
		protected virtual object CreateResult()
		{
			return Activator.CreateInstance<TModel>();
		}

		protected virtual object ChangeModel(TModel model)
		{
			return model;
		}

		protected virtual TCondition CreateCondition()
		{
			return null;
		}

		protected virtual IDictionary<string, object> GetKeys(string values)
		{
			var properties = TypeDescriptor.GetProperties(typeof(TModel), new Attribute[] { new KeyAttribute() });
			IDictionary<string, object> result = new Dictionary<string, object>();

			if(properties != null && properties.Count > 0)
			{
				foreach(PropertyDescriptor property in properties)
				{
					if(property.PropertyType == typeof(string))
						result[property.Name] = this.ControllerContext.RouteData.Values["id"];
					else
						result[property.Name] = Zongsoft.Common.Convert.ConvertValue(this.ControllerContext.RouteData.Values["id"], property.PropertyType);
				}
			}

			return result;
		}

		protected virtual IObjectAccess CreateObjectAccess()
		{
			if(_serviceProviderFactory == null)
				return null;

			return _serviceProviderFactory.Default.Resolve<IObjectAccess>();
		}

		protected virtual void OnIndexing(TCondition condition)
		{
		}

		protected virtual object OnIndexed(TCondition condition, IEnumerable<TModel> models)
		{
			return models;
		}

		protected virtual void OnUpdated(TModel model)
		{
		}

		protected virtual void OnUpdating(TModel model)
		{
		}
		#endregion

		#region 列表数据
		public ActionResult Index(int? pageIndex)
		{
			if(pageIndex.HasValue)
				this.Condition.PageIndex = pageIndex.Value;
			this.Condition.PageIndex = Math.Max(this.Condition.PageIndex, 1);

			return this.Index(this.Condition);
		}

		[HttpPost]
		public ActionResult Index(TCondition condition)
		{
			if(condition == null)
				return this.View();

			condition.PageIndex = Math.Max(condition.PageIndex, 1);
			this.TempData[_conditionKey] = condition;

			this.OnIndexing(condition);
			var models = this.ObjectAccess.Select<TModel>(this.Condition);
			var result = this.OnIndexed(condition, models);

			this.ViewData["Condition"] = this.Condition;
			return this.View(result);
		}
		#endregion

		#region 详细信息
		//public ActionResult Details()
		//{
		//    IDictionary<string, object> inParameters = this.GetKeys(id);
		//    IDictionary<string, object> outParameters;

		//    var result = this.ObjectAccess.Select<TModel>(inParameters, out outParameters).FirstOrDefault();

		//    //将返回参数设置到当前视图数据中
		//    this.SetViewData(outParameters);

		//    return this.View(result);
		//}

		//public ActionResult Details(string id)
		//{
		//    IDictionary<string, object> inParameters = this.GetKeys(id);
		//    IDictionary<string, object> outParameters;

		//    var result = this.ObjectAccess.Select<TModel>(inParameters, out outParameters).FirstOrDefault();

		//    //将返回参数设置到当前视图数据中
		//    this.SetViewData(outParameters);

		//    return this.View(result);
		//}
		#endregion

		#region 删除方法
		//[HttpDelete, HttpGet]
		//public ActionResult Delete(string id)
		//{
		//    IDictionary<string, object> parameters = this.GetKeys(id);

		//    if(parameters == null || parameters.Count < 1)
		//        return this.View();

		//    this.ObjectAccess.Delete(this.Name, parameters);

		//    return this.RedirectToAction("Index");
		//}

		//[HttpPost]
		//public ActionResult Delete(string[] keys)
		//{
		//    if(condition == null)
		//        return this.View();

		//    this.ObjectAccess.Delete(condition);

		//    return this.RedirectToAction("Index");
		//}
		#endregion

		#region 创建方法
		public ActionResult Create()
		{
			object result = this.CreateResult();

			return this.View(result);
		}

		[HttpPost]
		public ActionResult Create(TModel model)
		{
			this.OnUpdating(model);

			ResetModelState(model);

			if(!ModelState.IsValid)
				return this.View(ChangeModel(model));

			this.ObjectAccess.Insert(model);
			this.OnUpdated(model);

			if(string.IsNullOrWhiteSpace(this.Request.Form["ContinueCreate"]))
				return this.RedirectToAction("Index");
			else
				return this.Create();
		}
		#endregion

		#region 编辑修改
		//public ActionResult Edit(string id)
		//{
		//    IDictionary<string, object> inParameters = this.GetKeys(id);
		//    IDictionary<string, object> outParameters;

		//    if(inParameters == null || inParameters.Count < 1)
		//        return this.View();

		//    var result = this.ObjectAccess.Select<TModel>(inParameters, out outParameters).FirstOrDefault();

		//    //将返回参数设置到当前视图数据中
		//    this.SetViewData(outParameters);

		//    return this.View(result);
		//}

		[HttpPost]
		public ActionResult Edit(TModel model)
		{
			this.OnUpdating(model);

			ResetModelState(model);

			if(!this.ModelState.IsValid)
				return this.View(ChangeModel(model));

			this.ObjectAccess.Update(model);
			this.OnUpdated(model);

			if(string.IsNullOrWhiteSpace(this.Request.Form["ContinueEdit"]))
				return this.RedirectToAction("Index");
			else
				return this.View(ChangeModel(model));
		}
		#endregion

		#region 保护方法
		protected void SetViewData(IDictionary<string, object> data)
		{
			if(data == null || data.Count < 1)
				return;

			foreach(var entry in data)
			{
				this.ViewData[entry.Key] = entry.Value;
			}
		}
		#endregion

		#region 私有方法
		private string GetConditionKey()
		{
			return this.GetType().FullName;
		}

		private void ResetModelState(TModel model)
		{
			var result=new List<ValidationResult>();
			var flag=Validator.TryValidateObject(model, new ValidationContext(model, null, null), result,true);

			var error = result.ToDictionary(p => p.MemberNames.FirstOrDefault(), p => p.ErrorMessage);

			foreach(var key in ModelState.Keys)
			{
				ModelState[key].Errors.Clear();
				if(error.ContainsKey(key))
					ModelState[key].Errors.Add(error[key]);
			}
		}
		#endregion
	}
}
