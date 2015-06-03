using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	public class DataItemContainer<TOwner> : Literal, IDataItemContainer where TOwner : CompositeDataBoundControl
	{
		#region 成员字段
		private TOwner _owner;
		private object _dataItem;
		private int _index;
		private int _displayIndex;
		#endregion

		#region 构造函数
		internal DataItemContainer(TOwner owner, object dataItem, int index, string tagName = null, string cssClass = "item") : this(owner, dataItem, index, index, tagName, cssClass)
		{
		}

		internal DataItemContainer(TOwner owner, object dataItem, int index, int displayIndex, string tagName = null, string cssClass = "item") : base(tagName, cssClass)
		{
			if(owner == null)
				throw new ArgumentNullException("owner");

			_owner = owner;
			_dataItem = dataItem;
			_index = index;
			_displayIndex = displayIndex;
		}
		#endregion

		#region 公共属性
		public TOwner Owner
		{
			get
			{
				return _owner;
			}
		}

		public object DataSource
		{
			get
			{
				return _owner.DataSource;
			}
		}

		public object DataItem
		{
			get
			{
				return _dataItem;
			}
		}

		public int Index
		{
			get
			{
				return _index;
			}
		}

		public int DisplayIndex
		{
			get
			{
				return _displayIndex;
			}
		}
		#endregion

		#region 显式实现
		int IDataItemContainer.DataItemIndex
		{
			get
			{
				return _index;
			}
		}
		#endregion
	}
}
