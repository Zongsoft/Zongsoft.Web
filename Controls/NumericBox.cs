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
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace Zongsoft.Web.Controls
{
	public class NumericBox : TextBoxBase
	{
		#region 构造函数
		public NumericBox()
		{
			this.CssClass = "numeric";
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取或设置数字部分小数点的位数。
		/// </summary>
		[DefaultValue(0)]
		public byte Scale
		{
			get
			{
				return this.GetAttributeValue<byte>("Scale", 0);
			}
			set
			{
				this.SetAttributeValue(() => this.Scale, value);
			}
		}

		/// <summary>
		/// 获取或设置数字精度，即数字的总位数。
		/// </summary>
		[DefaultValue(0)]
		public byte Precision
		{
			get
			{
				return this.GetAttributeValue<byte>("Precision", 0);
			}
			set
			{
				this.SetAttributeValue(() => this.Precision, value);
			}
		}

		[DefaultValue(1)]
		public double Increment
		{
			get
			{
				return this.GetAttributeValue<double>("Increment", 1);
			}
			set
			{
				this.SetAttributeValue(() => this.Increment, value);
			}
		}

		[DefaultValue(0)]
		public double Maximum
		{
			get
			{
				return this.GetAttributeValue<double>("Maximum", 0);
			}
			set
			{
				this.SetAttributeValue(() => this.Maximum, value);
			}
		}

		[DefaultValue(0)]
		public double Minimum
		{
			get
			{
				return this.GetAttributeValue<double>("Minimum", 0);
			}
			set
			{
				this.SetAttributeValue(() => this.Minimum, value);
			}
		}
		#endregion
	}
}
