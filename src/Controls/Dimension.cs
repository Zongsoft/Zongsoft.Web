/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015 Zongsoft Corporation <http://www.zongsoft.com>
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
	/// <summary>
	/// 表示尺寸大小的定义。
	/// </summary>
	public enum Dimension
	{
		/// <summary>未指定</summary>
		None,
		/// <summary>迷你</summary>
		Mini,
		/// <summary>很小</summary>
		Tiny,
		/// <summary>较小</summary>
		Small,
		/// <summary>中等</summary>
		Medium,
		/// <summary>较大</summary>
		Large,
		/// <summary>很大</summary>
		Big,
		/// <summary>巨大</summary>
		Huge,
		/// <summary>最大</summary>
		Massive,
	}
}
