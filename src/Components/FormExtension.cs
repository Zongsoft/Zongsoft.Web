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
		public static object GetDataIndex(this TemplateControl container)
		{
			if(container == null)
				throw new ArgumentNullException("container");

			var page = (container as Page) ?? container.Page;
			var stack = GetDataContainer(page);

			if(stack != null && stack.Count > 0)
				return stack.Peek().Index;

			return -1;
		}

		public static object GetDataItem(this TemplateControl container)
		{
			if(container == null)
				throw new ArgumentNullException("container");

			var page = (container as Page) ?? container.Page;
			var stack = GetDataContainer(page);

			if(stack != null && stack.Count > 0)
				return stack.Peek().Data;

			return null;
		}

		public static object GetDataContext(this TemplateControl container)
		{
			return GetDataItem(container);
		}
		#endregion

		#region 内部方法
		internal static void PushDataItem(Page page, object dataItem, int index)
		{
			if(dataItem == null)
				return;

			var stack = GetDataContainer(page);
			stack.Push(new DataContext(dataItem, index));
		}

		internal static DataContext PopDataItem(Page page)
		{
			var stack = GetDataContainer(page);

			if(stack != null && stack.Count > 0)
				return stack.Pop();

			return null;
		}
		#endregion

		#region 私有方法
		private static Stack<DataContext> GetDataContainer(Page page)
		{
			if(!page.Items.Contains(DATACONTEXT_KEY))
				page.Items[DATACONTEXT_KEY] = new Stack<DataContext>();

			return page.Items[DATACONTEXT_KEY] as Stack<DataContext>;
		}
		#endregion

		#region 嵌套子类
		internal class DataContext
		{
			public object Data;
			public int Index;

			internal DataContext(object data, int index)
			{
				this.Data = data;
				this.Index = index;
			}
		}
		#endregion
	}
}
