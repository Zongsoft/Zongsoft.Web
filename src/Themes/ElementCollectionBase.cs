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

namespace Zongsoft.Web.Themes
{
	public abstract class ElementCollectionBase<T, TOwner> : Zongsoft.Collections.NamedCollectionBase<T> where T : ElementBase<TOwner>
	{
		private TOwner _owner;

		protected ElementCollectionBase(TOwner owner) : base(StringComparer.OrdinalIgnoreCase)
		{
			if(owner == null)
				throw new ArgumentNullException("owner");

			_owner = owner;
		}

		public TOwner Owner
		{
			get
			{
				return _owner;
			}
		}

		protected override string GetKeyForItem(T item)
		{
			return item.Name;
		}

		protected override void InsertItems(int index, IEnumerable<T> items)
		{
			if(items != null)
			{
				foreach(var item in items)
				{
					item.Owner = _owner;
				}
			}

			base.InsertItems(index, items);
		}

		protected override void SetItem(int index, T item)
		{
			var oldItem = base[index];

			if(oldItem != null)
				oldItem.Owner = default(TOwner);

			if(item != null)
				item.Owner = _owner;

			base.SetItem(index, item);
		}

		protected override void RemoveItem(int index)
		{
			var oldItem = base[index];

			if(oldItem != null)
				oldItem.Owner = default(TOwner);

			base.RemoveItem(index);
		}

		protected override void ClearItems()
		{
			foreach(var item in base.Items)
				item.Owner = default(TOwner);

			base.ClearItems();
		}
	}
}
