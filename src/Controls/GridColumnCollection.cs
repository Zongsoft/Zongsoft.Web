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
using System.Linq;
using System.Text;

namespace Zongsoft.Web.Controls
{
	public class GridColumnCollection : Zongsoft.Collections.NamedCollectionBase<GridColumnBase>
	{
		public GridColumnCollection() : base(StringComparer.OrdinalIgnoreCase)
		{
		}

		protected override string GetKeyForItem(GridColumnBase item)
		{
			return item.Name;
		}

		public double GetTotalWeight()
		{
			double totalWeight = 0;

			foreach(var column in this)
			{
				if(column.Width.Type == UnitType.None)
					totalWeight += column.Width.Value;
			}

			return totalWeight;
		}
	}
}
