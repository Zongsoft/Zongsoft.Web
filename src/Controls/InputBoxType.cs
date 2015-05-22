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
using System.ComponentModel;

namespace Zongsoft.Web.Controls
{
	public enum InputBoxType
	{
		[Category("Text")]
		Text,
		[Category("Text")]
		Password,

		[Category("Button")]
		Button,
		[Category("Button")]
		Reset,
		[Category("Button")]
		Submit,

		[Category("Button")]
		CheckBox,
		[Category("Button")]
		Radio,

		Hidden,
		Image,
		File,

		/// <summary>扩展：数字输入框</summary>
		[Category("Text")]
		Number,
		/// <summary>扩展：日期输入框</summary>
		[Category("Text")]
		Date,
		/// <summary>扩展：时间输入框</summary>
		[Category("Text")]
		Time,
		/// <summary>扩展：日期和时间输入框</summary>
		[Category("Text")]
		DateTime,
		/// <summary>扩展：网址输入框</summary>
		[Category("Text")]
		Url,
		/// <summary>扩展：电子邮箱地址输入框</summary>
		[Category("Text")]
		Email,
		/// <summary>扩展：搜索框</summary>
		[Category("Text")]
		Search,
	}
}
