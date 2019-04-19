/*
 *   _____                                ______
 *  /_   /  ____  ____  ____  _________  / __/ /_
 *    / /  / __ \/ __ \/ __ \/ ___/ __ \/ /_/ __/
 *   / /__/ /_/ / / / / /_/ /\_ \/ /_/ / __/ /_
 *  /____/\____/_/ /_/\__  /____/\____/_/  \__/
 *                   /____/
 *
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@qq.com>
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
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Zongsoft.Data;

namespace Zongsoft.Web.Http
{
	public class HttpControllerBase<TModel, TConditional, TService> : HttpControllerBase<TModel, TService>
	                                                                  where TModel : class
	                                                                  where TConditional : class, IEntity
	                                                                  where TService : class, IDataService<TModel>
	{
		#region 构造函数
		protected HttpControllerBase(Zongsoft.Services.IServiceProvider serviceProvider) : base(serviceProvider)
		{
		}
		#endregion

		#region 公共方法
		[HttpPost]
		public virtual object Count(TConditional conditional)
		{
			if(conditional == null)
				return this.DataService.Count(null);
			else
				return this.DataService.Count(Conditional.ToCondition(conditional));
		}

		[HttpPost]
		public virtual object Exists(TConditional conditional)
		{
			bool existed = false;

			if(conditional == null)
				existed = this.DataService.Exists(null);
			else
				existed = this.DataService.Exists(Conditional.ToCondition(conditional));

			if(existed)
				return this.Ok();
			else
				return this.NotFound();
		}

		[HttpGet]
		public virtual object Search(string keyword, [FromUri]Paging paging = null)
		{
			var searcher = this.DataService.Searcher;

			if(searcher == null)
				return this.BadRequest("This resource does not support the search operation.");

			if(string.IsNullOrWhiteSpace(keyword))
				this.BadRequest("Missing keyword for search.");

			return this.GetResult(searcher.Search(keyword, this.GetSchema(), paging));
		}

		[HttpPost]
		public virtual object Query(TConditional conditional, [FromUri]Paging paging = null)
		{
			return this.GetResult(this.DataService.Select(Conditional.ToCondition(conditional), this.GetSchema(), paging));
		}
		#endregion
	}
}