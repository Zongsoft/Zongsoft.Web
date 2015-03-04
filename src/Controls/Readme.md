数据提取框（Zongsoft.Web.Controls.Picker）
=========================================

服务器代码：
<zs:Picker id="Creator"
	Title=""
	Label=""
	Width=""
	Height=""
	Enabled="true"
	DataKeys="PropertyName, PropertyName"
	FormatString="{PropertyName}-{PropertyName}"
	Url="/Common/Road/Get?Keyword=&Type=Employee&PageIndex=1" runat="server">
	<Columns>
		<zs:PickerColumn name="" Title="" Width="" />
		<zs:PickerColumn name="" Title="" Width="" />
	</Columns>
</zs:Picker>

客户端代码：
<label for="ID-pickbox">LabelText</label>
<input id="ID-pickbox" name="ID-pickbox" class="pickbox" value="formatted value" zs:format="{Property}-{Property}" />
<div id="ID-view" title="" style="display:none">
	<table width="" height="">
		<thead>
			<tr>
				<td name="PropertyName" width="x%">Property-Title</td>
				<td name="PropertyName" width="x%">Property-Title</td>
			</tr>
		</thead>
	</table>

	<input id="ID" name="ID" type="hidden" class="picker" value="value of the key"
		   zs:selectionMode="single"
		   zs:keys="KeyProperty, KeyProperty"
		   zs:url="/Tollgates/Picker/Get=type=Road&values=..." />
</div>


下拉组合框(Zongsoft.Web.Controls.ComboBox)
=========================================

客户端代码：
<label for="ID-combobox">LabelText</label>
<input id="ID-combobox" name="ID-combobox" type="text" class="combobox-input" value="" />
<div id="ID-view" style="display:none">
	<ul>
		<li value="" disabled="disabled">text</li>
	</ul>

	<input id="" name="" type="hidden" class="combobox" value="" />
</div>

图表控件(Zongsoft.Web.Controls.Chart)
====================================
客户端代码：
<input id="ID_data" name="ID_data" type="hidden" value="JSON" />
<input id="ID_series" name="ID_series" type="hidden" value="JSON" />
<div id="ID" width="" height="" title="" class="chart-pie">
</div>

文件上传控件(Zongsoft.Web.Controls.FileUpload)
=============================================
客户端代码：
<input id="ID" name="ID" type="file" value="" style="display:none" />
<input id="ID_input" name="ID_input" type="text" class="file" value="" />
