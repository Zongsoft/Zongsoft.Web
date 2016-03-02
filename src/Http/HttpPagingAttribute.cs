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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;

namespace Zongsoft.Web.Http
{
	public class HttpPagingAttribute : ActionFilterAttribute
	{
		#region 常量定义
		private const string PAGING_PARAMETER_NAME = "paging";
		#endregion

		#region 重写方法
		public override void OnActionExecuted(HttpActionExecutedContext context)
		{
			if(context.Response.IsSuccessStatusCode)
			{
				//获取当前操作是否支持数据分页加载
				var paging = this.GetPaging(context.ActionContext.ActionArguments);

				if(paging != null)
				{
					//注意：由于数据可能未被加载，因此分页结果尚未计算；故此需要显式加载数据以驱动分页计算
					context.Response.Content.LoadIntoBufferAsync();

					if(paging.PageCount > 0)
					{
						context.Response.Content.Headers.Add("x-paging-size", paging.PageSize.ToString());
						context.Response.Content.Headers.Add("x-paging-index", paging.PageIndex.ToString());
						context.Response.Content.Headers.Add("x-paging-count", paging.PageCount.ToString());
						context.Response.Content.Headers.Add("x-result-totalCount", paging.TotalCount.ToString());
					}
				}
			}

			//调用基类同名方法
			base.OnActionExecuted(context);
		}
		#endregion

		#region 私有方法
		private Zongsoft.Data.Paging GetPaging(IDictionary<string, object> parameters)
		{
			object result;

			if(parameters.TryGetValue(PAGING_PARAMETER_NAME, out result) && result is Zongsoft.Data.Paging)
				return (Zongsoft.Data.Paging)result;

			foreach(var parameter in parameters)
			{
				if(parameter.Value is Zongsoft.Data.Paging)
					return (Zongsoft.Data.Paging)parameter.Value;
			}

			return null;
		}
		#endregion
	}
}