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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zongsoft.Web.Controls
{
	public class Property
	{
		#region 成员字段
		private string _name;
		private string _value;
		private IDataBoundControlPart _owner;
		#endregion

		#region 公共属性
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();

				_name = value.Trim();
			}
		}

		public string Value
		{
			get
			{
				if(_owner == null)
					return _value;

				return BindingUtility.FormatBindingValue(_value, _owner.BindingSource);
			}
			set
			{
				_value = value;
			}
		}
		#endregion

		#region 内部属性
		internal IDataBoundControlPart Owner
		{
			get
			{
				return _owner;
			}
			set
			{
				_owner = value;
			}
		}
		#endregion
	}
}
