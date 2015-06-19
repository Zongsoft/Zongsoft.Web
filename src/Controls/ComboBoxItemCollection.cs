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
using System.Collections.ObjectModel;
using System.Text;

namespace Zongsoft.Web.Controls
{
	public class ComboBoxItemCollection : Zongsoft.Collections.Collection<ComboBoxItem>
	{
		private ComboBox _owner;

		public ComboBoxItemCollection(ComboBox owner)
		{
			if(owner == null)
				throw new ArgumentNullException("owner");

			_owner = owner;
		}

		protected override void InsertItems(int index, IEnumerable<ComboBoxItem> items)
		{
			if(items == null)
				return;

			foreach(var item in items)
			{
				if(item != null)
					item.ComboBox = _owner;
			}

			base.InsertItems(index, items);
		}
	}
}
