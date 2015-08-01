using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	public static class FormExtension
	{
		#region 常量定义
		private const string DATACONTEXT_KEY = "__Zongsoft.Web.Controls.DataContext__";
		#endregion

		#region 扩展方法
		public static object GetDataContext(this Page page)
		{
			var stack = GetDataContainer(page);

			if(stack != null && stack.Count > 0)
				return stack.Peek();

			return null;
		}
		#endregion

		#region 公共方法
		public static void PushDataItem(Page page, object dataItem)
		{
			if(dataItem == null)
				return;

			var stack = GetDataContainer(page);
			stack.Push(dataItem);
		}

		public static object PopDataItem(Page page)
		{
			var stack = GetDataContainer(page);

			if(stack != null && stack.Count > 0)
				return stack.Pop();

			return null;
		}
		#endregion

		#region 私有方法
		private static Stack<object> GetDataContainer(Page page)
		{
			if(!page.Items.Contains(DATACONTEXT_KEY))
				page.Items[DATACONTEXT_KEY] = new Stack<object>();

			return page.Items[DATACONTEXT_KEY] as Stack<object>;
		}
		#endregion
	}
}
