using System;
using System.ComponentModel;

namespace Zongsoft.Web.Controls
{
	/// <summary>
	/// 表示列表视图的类型。
	/// </summary>
	public enum ListViewType
	{
		/// <summary>不生成列表标签</summary>
		None,

		/// <summary>生成&lt;dl&gt;标签</summary>
		List,

		/// <summary>生成&lt;ul&gt;标签</summary>
		BulletList,

		/// <summary>生成&lt;ol&gt;标签</summary>
		OrderedList,
	}
}
