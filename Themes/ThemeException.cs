﻿/*
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
using System.Runtime.Serialization;

namespace Zongsoft.Web.Themes
{
	[Serializable]
	public class ThemeException : Exception
	{
		#region 成员变量
		private int _failureCode;
		#endregion

		#region 构造函数
		public ThemeException() : this(string.Empty, null)
		{
		}

		public ThemeException(string message) : this(message, null)
		{
		}

		public ThemeException(string message, Exception innerException) : base(message, innerException)
		{
			_failureCode = 0;
		}

		public ThemeException(int failureCode, string message) : this(failureCode, message, null)
		{
		}

		public ThemeException(int failureCode, string message, Exception innerException) : base(message, innerException)
		{
			_failureCode = failureCode;
		}

		protected ThemeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			_failureCode = info.GetInt32("FailureCode");
		}
		#endregion

		#region 公共属性
		public int FailureCode
		{
			get
			{
				return _failureCode;
			}
		}
		#endregion

		#region 重写方法
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("FailureCode", _failureCode);
		}
		#endregion
	}
}
