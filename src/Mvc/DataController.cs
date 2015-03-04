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
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
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
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Zongsoft.Data;
using Zongsoft.Data.Entities;
using Zongsoft.Services;

namespace Zongsoft.Web.Mvc
{
	[Obsolete]
	public class DataController : AuthorizationController
	{
		#region 成员变量
		private IObjectAccess _objectAccess;
		private IServiceProviderFactory _serviceProviderFactory;
		#endregion

		#region 构造函数
		public DataController(IServiceProviderFactory serviceProviderFactory):base(serviceProviderFactory)
		{
			if(serviceProviderFactory == null)
				throw new ArgumentNullException("serviceProviderFactory");

			_serviceProviderFactory = serviceProviderFactory;
		}
		#endregion

		#region 公共属性
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
		protected virtual IObjectAccess CreateObjectAccess()
		{
			if(_serviceProviderFactory == null)
				return null;

			return _serviceProviderFactory.Default.Resolve<IObjectAccess>();
		}
		#endregion
	}
}
