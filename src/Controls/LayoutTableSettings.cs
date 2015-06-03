using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace Zongsoft.Web.Controls
{
	public class LayoutTableSettings
	{
		#region 成员字段
		private bool _mergeLastCells;
		#endregion

		#region 构造函数
		public LayoutTableSettings()
		{
			_mergeLastCells = true;
		}
		#endregion

		#region 公共属性
		[DefaultValue(true)]
		public bool MergeLastCells
		{
			get
			{
				return _mergeLastCells;
			}
			set
			{
				_mergeLastCells = value;
			}
		}
		#endregion
	}
}
