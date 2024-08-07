"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[18],{92709:function(e,t,a){a.d(t,{LO:function(){return i},ji:function(){return l},uk:function(){return o}});const l={0:"TCP/Socket",1:"RESTFul API"},i={0:{label:"UNKNOWN",labelCN:"未知",color:"rgb(64, 158, 255)"},1:{label:"IDLE",labelCN:"閒置",color:"orange"},2:{label:"RUN",labelCN:"執行",color:"green"},3:{label:"DOWN",labelCN:"當機",color:"red"},4:{label:"Charging",labelCN:"充電",color:"#0d6efd"}},o={0:{label:"FORK",labelCN:"叉車",color:"rgb(64, 158, 255)"},1:{label:"YUNTECH-FORK",labelCN:"叉車(雲科)",color:"orange"},2:{label:"INSPECTOIN",labelCN:"巡檢",color:"green"},3:{label:"SUBMARINE",labelCN:"潛盾",color:"red"},4:{label:"PARTS",labelCN:"Parts",color:"blue"}}},26018:function(e,t,a){a.r(t),a.d(t,{default:function(){return ne}});var l={};a.r(l);var i=a(66252);const o={class:"vehicles"},n={class:""},r={class:""},s={class:""};function d(e,t,a,l,d,u){const c=(0,i.up)("VehicleListTable"),m=(0,i.up)("VehicleControlVue"),w=(0,i.up)("b-tab"),h=(0,i.up)("VehicleMaintain"),V=(0,i.up)("AddVehicle"),p=(0,i.up)("b-tabs"),g=(0,i.up)("b-card");return(0,i.wg)(),(0,i.iD)("div",o,[(0,i.Wm)(g,{"no-body":""},{default:(0,i.w5)((()=>[(0,i.Wm)(p,{pills:"",vertical:"",justified:"","nav-class":"my-nav","content-class":"my-nav-tabs"},{default:(0,i.w5)((()=>[(0,i.Wm)(w,{title:"車輛列表",active:""},{default:(0,i.w5)((()=>[(0,i._)("div",n,[(0,i.Wm)(c),u.isDeveloperUser?((0,i.wg)(),(0,i.j4)(m,{key:0})):(0,i.kq)("",!0)])])),_:1}),(0,i.Wm)(w,{title:"維修保養"},{default:(0,i.w5)((()=>[(0,i._)("div",r,[(0,i.Wm)(h)])])),_:1}),(0,i.Wm)(w,{title:"新增車輛"},{default:(0,i.w5)((()=>[(0,i._)("div",s,[(0,i.Wm)(V)])])),_:1})])),_:1})])),_:1})])}var u=a(3577);const c={class:"add-vehicle w-100"},m={class:"border-top py-2 text-start"};function w(e,t,a,l,o,n){const r=(0,i.up)("el-divider"),s=(0,i.up)("el-input"),d=(0,i.up)("el-form-item"),w=(0,i.up)("el-option"),h=(0,i.up)("el-select"),V=(0,i.up)("el-switch"),p=(0,i.up)("el-form"),g=(0,i.up)("b-button");return(0,i.wg)(),(0,i.iD)("div",c,[(0,i.Wm)(p,{"label-width":"100px","label-position":"left"},{default:(0,i.w5)((()=>[(0,i.Wm)(r,null,{default:(0,i.w5)((()=>[(0,i.Uk)("Basic")])),_:1}),(0,i.Wm)(d,{label:"車輛ID"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{class:"add-vehicle-input",modelValue:o.payload.AGV_Name,"onUpdate:modelValue":t[0]||(t[0]=e=>o.payload.AGV_Name=e)},null,8,["modelValue"])])),_:1}),(0,i.Wm)(d,{label:"車輛顯示名稱"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{class:"add-vehicle-input"})])),_:1}),(0,i.Wm)(d,{label:"車輛類型"},{default:(0,i.w5)((()=>[(0,i.Wm)(h,{class:"add-vehicle-input",modelValue:o.payload.Model,"onUpdate:modelValue":t[1]||(t[1]=e=>o.payload.Model=e)},{default:(0,i.w5)((()=>[(0,i.Wm)(w,{label:"叉車 AGV",value:0}),(0,i.Wm)(w,{label:"巡檢 AGV",value:2}),(0,i.Wm)(w,{label:"潛盾 AGV",value:3}),(0,i.Wm)(w,{label:"Parts AGV",value:4})])),_:1},8,["modelValue"])])),_:1}),(0,i.Wm)(d,{label:"通訊方式"},{default:(0,i.w5)((()=>[(0,i.Wm)(h,{class:"add-vehicle-input",modelValue:o.payload.Protocol,"onUpdate:modelValue":t[2]||(t[2]=e=>o.payload.Protocol=e)},{default:(0,i.w5)((()=>[(0,i.Wm)(w,{label:"TCP/Socket",value:0}),(0,i.Wm)(w,{label:"RESTFul API",value:1})])),_:1},8,["modelValue"])])),_:1}),(0,i.Wm)(d,{label:"IP"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{class:"add-vehicle-input",modelValue:o.payload.IP,"onUpdate:modelValue":t[3]||(t[3]=e=>o.payload.IP=e)},null,8,["modelValue"])])),_:1}),(0,i.Wm)(d,{label:"Port"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{class:"add-vehicle-input",modelValue:o.payload.Port,"onUpdate:modelValue":t[4]||(t[4]=e=>o.payload.Port=e)},null,8,["modelValue"])])),_:1}),(0,i.Wm)(r,null,{default:(0,i.w5)((()=>[(0,i.Uk)("Layout")])),_:1}),(0,i.Wm)(d,{label:"車輛長度(cm)"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{class:"add-vehicle-input",modelValue:o.payload.VehicleLength,"onUpdate:modelValue":t[5]||(t[5]=e=>o.payload.VehicleLength=e)},null,8,["modelValue"])])),_:1}),(0,i.Wm)(d,{label:"車輛寬度(cm)"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{class:"add-vehicle-input",modelValue:o.payload.VehicleWidth,"onUpdate:modelValue":t[6]||(t[6]=e=>o.payload.VehicleWidth=e)},null,8,["modelValue"])])),_:1}),(0,i.Wm)(r,null,{default:(0,i.w5)((()=>[(0,i.Uk)("Developer")])),_:1}),(0,i.Wm)(d,{label:"模擬"},{default:(0,i.w5)((()=>[(0,i.Wm)(V,{class:"add-vehicle-input",modelValue:o.payload.Simulation,"onUpdate:modelValue":t[7]||(t[7]=e=>o.payload.Simulation=e)},null,8,["modelValue"])])),_:1})])),_:1}),(0,i._)("div",m,[(0,i.Wm)(g,{onClick:t[8]||(t[8]=e=>n.IsEditMode?n.EditVehicle():n.AddVehicle()),variant:"primary",loading:o.adding,style:{width:"120px"}},{default:(0,i.w5)((()=>[(0,i.Uk)((0,u.zw)(n.btnText),1)])),_:1},8,["loading"])])])}var h=a(38418),V={props:{mode:{type:String,default:"add"}},computed:{IsEditMode(){return"edit"==this.mode},btnText(){return this.IsEditMode?"修改":"新增"}},data(){return{payload:{AGV_Name:"AGV_",Model:3,Protocol:0,IP:"127.0.0.1",Port:7025,VehicleLength:145,VehicleWidth:70,Simulation:!1,MaintainSettings:[]},oriAGVID:"",adding:!1}},methods:{async AddVehicle(){this.adding=!0;var e=await h.fO.AddVehicle(this.payload);this.adding=!1,e.confirm?this.$swal.fire({text:"",title:"新增成功",icon:"success",showCancelButton:!1,confirmButtonText:"OK",customClass:"top-most-sweetalert"}):this.$swal.fire({text:e.message,title:"新增失敗",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"top-most-sweetalert"})},async EditVehicle(){var e=await h.fO.EditVehicle(this.payload,this.oriAGVID);e.confirm?this.$swal.fire({text:"",title:"修改成功",icon:"success",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}):this.$swal.fire({text:e.message,title:"修改失敗",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"})},UpdatePayload(e){this.payload=e,this.oriAGVID=e.AGV_Name}}},p=a(83744);const g=(0,p.Z)(V,[["render",w],["__scopeId","data-v-e2d9caf4"]]);var f=g,_=a(49963);const b={class:"vehicle-list-table"},v={class:"text-start"},y={class:""};function W(e,t,a,l,o,n){const r=(0,i.up)("el-table-column"),s=(0,i.up)("el-tag"),d=(0,i.up)("el-checkbox"),c=(0,i.up)("el-button"),m=(0,i.up)("el-table"),w=(0,i.up)("AddVehicle"),h=(0,i.up)("el-drawer");return(0,i.wg)(),(0,i.iD)("div",b,[(0,i.Wm)(m,{"header-cell-class-name":"my-el-table-cell-class","row-key":"AGV_Name",border:"","header-cell-style":n.tableHeaderStyle,data:n.GetAGVStatesData,size:"large"},{default:(0,i.w5)((()=>[(0,i.Wm)(r,{label:"AGV ID",prop:"AGV_Name",width:"130",align:"center"}),(0,i.Wm)(r,{label:"類型",prop:"Model",width:"90",align:"center"},{default:(0,i.w5)((e=>[(0,i.Wm)(s,{effect:"dark"},{default:(0,i.w5)((()=>[(0,i.Uk)((0,u.zw)(n.VehicleModels[e.row.Model].labelCN),1)])),_:2},1024)])),_:1}),(0,i.Wm)(r,{label:"當前狀態",prop:"MainStatus",width:"90",align:"center"},{default:(0,i.w5)((e=>[e.row.Connected?((0,i.wg)(),(0,i.j4)(s,{key:1,effect:"dark",color:n.AGVMainStatus[e.row.MainStatus].color},{default:(0,i.w5)((()=>[(0,i.Uk)((0,u.zw)(n.AGVMainStatus[e.row.MainStatus].label),1)])),_:2},1032,["color"])):((0,i.wg)(),(0,i.j4)(s,{key:0,effect:"dark",type:"danger"},{default:(0,i.w5)((()=>[(0,i.Uk)("斷線")])),_:1}))])),_:1}),(0,i.Wm)(r,{label:"當前位置",prop:"CurrentLocation",align:"center",width:"100"}),(0,i.Wm)(r,{label:"通訊方式",prop:"Protocol",width:"120",align:"center"},{default:(0,i.w5)((e=>[(0,i.Wm)(s,null,{default:(0,i.w5)((()=>[(0,i.Uk)((0,u.zw)(n.ProtocolText[e.row.Protocol]),1)])),_:2},1024)])),_:1}),(0,i.Wm)(r,{label:"IP",prop:"IP",width:"150",align:"center"}),(0,i.Wm)(r,{label:"PORT",prop:"Port",width:"90"}),(0,i.Wm)(r,{label:"車長(cm)",prop:"VehicleLength",width:"100",align:"center"}),(0,i.Wm)(r,{label:"車寬(cm)",prop:"VehicleWidth",width:"100",align:"center"}),(0,i.Wm)(r,{label:"版本號",prop:"AppVersion",width:"120",align:"center"},{default:(0,i.w5)((e=>[(0,i.Wm)(s,null,{default:(0,i.w5)((()=>[(0,i.Uk)((0,u.zw)(e.row.AppVersion),1)])),_:2},1024)])),_:1}),(0,i.Wm)(r,{label:"啟用模擬",prop:"Simulation",width:"100",align:"center"},{default:(0,i.w5)((e=>[(0,i.Wm)(d,{disabled:!0,modelValue:e.row.Simulation,"onUpdate:modelValue":t=>e.row.Simulation=t},null,8,["modelValue","onUpdate:modelValue"])])),_:1}),(0,i.Wm)(r,{fixed:"right",label:"Operations","min-width":"160"},{default:(0,i.w5)((e=>[(0,i.Wm)(c,{type:"success",size:"small",onClick:(0,_.iM)((t=>n.edit_row(e.row)),["prevent"])},{default:(0,i.w5)((()=>[(0,i.Uk)("編輯")])),_:2},1032,["onClick"]),(0,i.Wm)(c,{type:"danger",size:"small",onClick:(0,_.iM)((t=>n.delete_row(e.row)),["prevent"])},{default:(0,i.w5)((()=>[(0,i.Uk)("刪除")])),_:2},1032,["onClick"])])),_:1})])),_:1},8,["header-cell-style","data"]),(0,i.Wm)(h,{"z-index":1,modelValue:o.ShowEditAGVPropertyDrawer,"onUpdate:modelValue":t[0]||(t[0]=e=>o.ShowEditAGVPropertyDrawer=e)},{header:(0,i.w5)((({})=>[(0,i._)("h3",v,(0,u.zw)(n.drawerText),1)])),default:(0,i.w5)((()=>[(0,i._)("div",y,[(0,i.Wm)(w,{ref:"AgvPropertyEditor",mode:"edit"},null,512)])])),_:1},8,["modelValue"])])}var S=a(24239),C=a(92709),k={components:{AddVehicle:f},inject:["tableHeaderStyle"],data(){return{table:[],selectAGVProertyToEdit:{},ShowEditAGVPropertyDrawer:!1}},computed:{GetAGVStatesData(){return S.sn.getters.AGVStatesData},ProtocolText(){return C.ji},AGVMainStatus(){return C.LO},VehicleModels(){return C.uk},drawerText(){return this.selectAGVProertyToEdit.AGV_Name}},methods:{edit_row(e){this.selectAGVProertyToEdit=e,this.ShowEditAGVPropertyDrawer=!0,setTimeout((()=>{this.$refs["AgvPropertyEditor"].UpdatePayload(e)}),1)},async delete_row(e){var t=async()=>{var t=await h.fO.DeleteVehicle(e.AGV_Name);t.confirm?this.$swal.fire({text:"",title:"刪除車輛成功",icon:"success",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}):this.$swal.fire({text:t.message,title:"刪除車輛失敗",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"})};this.$swal.fire({text:"",title:`確定要刪除車輛-${e.AGV_Name}?`,icon:"warning",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((e=>{e.isConfirmed&&t()}))}}};const A=(0,p.Z)(k,[["render",W],["__scopeId","data-v-67c15d1e"]]);var M=A;const G={class:"w-100"},x={class:"border-bottom my-2 py-1 text-start"},N={class:"w-100 d-flex value-display"},D={class:"flex-fill text-center"},O={class:"w-100 d-flex value-display"},U={class:"flex-fill text-center"},I={class:"w-100 d-flex value-display"},P={class:"flex-fill text-center"},T={class:"w-100 d-flex value-display"},B={class:"flex-fill text-center"},R={class:"d-flex w-100"};function $(e,t,a,l,o,n){const r=(0,i.up)("el-button"),s=(0,i.up)("el-table-column"),d=(0,i.up)("el-table"),c=(0,i.up)("el-input-number"),m=(0,i.up)("el-dialog"),w=(0,i.Q2)("loading");return(0,i.wg)(),(0,i.iD)("div",G,[(0,i._)("div",x,[(0,i.Wm)(r,{onClick:t[0]||(t[0]=e=>n.getSettings())},{default:(0,i.w5)((()=>[(0,i.Uk)("重新整理")])),_:1})]),(0,i.wy)(((0,i.wg)(),(0,i.j4)(d,{data:n.tableData,"header-cell-class-name":"my-el-table-cell-class","row-key":"agv_name",border:"","header-cell-style":n.tableHeaderStyle,size:"large"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{label:"AGV ID",prop:"agv_name"}),(0,i.Wm)(s,{label:"走行馬達"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{label:"目前累計里程",prop:"currentHorizonMotorVal"},{default:(0,i.w5)((e=>[(0,i._)("div",N,[(0,i._)("div",D,(0,u.zw)(e.row.currentHorizonMotorVal.toFixed(3))+" km",1),(0,i.Wm)(r,{onClick:t=>n.ResetCurrentValue(e.row.agv_name,100)},{default:(0,i.w5)((()=>[(0,i.Uk)("重設")])),_:2},1032,["onClick"])])])),_:1}),(0,i.Wm)(s,{label:"保養里程",prop:"horizonMotorMaintainVal"},{default:(0,i.w5)((e=>[(0,i._)("div",O,[(0,i._)("div",U,(0,u.zw)(e.row.horizonMotorMaintainVal),1),(0,i.Wm)(r,{onClick:t=>n.ShowSettingDialog(e.row.agv_name,100,e.row.horizonMotorMaintainVal)},{default:(0,i.w5)((()=>[(0,i.Uk)("設定")])),_:2},1032,["onClick"])])])),_:1}),(0,i.Wm)(s,{label:"前次保養時間"}),(0,i.Wm)(s,{label:"前次保養人員"})])),_:1}),(0,i.Wm)(s,{label:"牙叉皮帶"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{label:"目前累計里程",prop:"currentForkBeltVal"},{default:(0,i.w5)((e=>[(0,i._)("div",I,[(0,i._)("div",P,(0,u.zw)(e.row.currentForkBeltVal),1),(0,i.Wm)(r,{onClick:t=>n.ResetCurrentValue(e.row.agv_name,101)},{default:(0,i.w5)((()=>[(0,i.Uk)("重設")])),_:2},1032,["onClick"])])])),_:1}),(0,i.Wm)(s,{label:"保養里程",prop:"forkBeltMaintainVal"},{default:(0,i.w5)((e=>[(0,i._)("div",T,[(0,i._)("div",B,(0,u.zw)(e.row.forkBeltMaintainVal),1),(0,i.Wm)(r,{onClick:t=>n.ShowSettingDialog(e.row.agv_name,101,e.row.forkBeltMaintainVal)},{default:(0,i.w5)((()=>[(0,i.Uk)("設定")])),_:2},1032,["onClick"])])])),_:1}),(0,i.Wm)(s,{label:"前次保養時間"}),(0,i.Wm)(s,{label:"前次保養人員"}),(0,i.Wm)(s,{label:""},{default:(0,i.w5)((()=>[(0,i._)("template",null,[(0,i.Wm)(r,null,{default:(0,i.w5)((()=>[(0,i.Uk)("確認")])),_:1})])])),_:1})])),_:1})])),_:1},8,["data","header-cell-style"])),[[w,o.loading]]),(0,i.Wm)(m,{title:o.dialog.title,modelValue:o.settingValueDialogShow,"onUpdate:modelValue":t[4]||(t[4]=e=>o.settingValueDialogShow=e),draggable:"",modal:!1,width:"400"},{default:(0,i.w5)((()=>[(0,i._)("div",R,[(0,i.Wm)(c,{class:"mx-2",size:"large",modelValue:o.dialog.oriValue,"onUpdate:modelValue":t[1]||(t[1]=e=>o.dialog.oriValue=e)},null,8,["modelValue"]),(0,i.Wm)(r,{size:"large",type:"primary",onClick:t[2]||(t[2]=e=>n.SettingValue())},{default:(0,i.w5)((()=>[(0,i.Uk)("修改")])),_:1}),(0,i.Wm)(r,{size:"large",onClick:t[3]||(t[3]=()=>{o.settingValueDialogShow=!1})},{default:(0,i.w5)((()=>[(0,i.Uk)("取消")])),_:1})])])),_:1},8,["title","modelValue"])])}var z=a(9669),E=a.n(z),H=a(663),K=E().create({baseURL:H.Z.vms_host});const L={GetSettings:"api/VehicleMaintain",ResetCurrentVal:"api/VehicleMaintain/ResetCurrentValue",SettingMaintainValue:"api/VehicleMaintain/SettingMaintainValue"},Z={GetSettings:async()=>{var e=await K.get(`${L.GetSettings}`);return e.data},ResetCurrentValue:async(e="AGV_001",t=100)=>{var a=await K.post(`${L.ResetCurrentVal}?agvName=${e}&item=${t}`);return a.data},SettingMaintainValue:async(e="AGV_001",t=100,a=0)=>{var l=await K.post(`${L.SettingMaintainValue}?agvName=${e}&item=${t}&value=${a}`);return l.data}};var j=Z,F=a(53259),q=a(49996),Q={inject:["tableHeaderStyle"],computed:{tableData(){var e=Object.keys(this.maintainStates),t=e.map((e=>({agv_name:e,currentHorizonMotorVal:this.maintainStates[e][0].currentValue,horizonMotorMaintainVal:this.maintainStates[e][0].maintainValue,currentForkBeltVal:this.maintainStates[e][1].currentValue,forkBeltMaintainVal:this.maintainStates[e][1].maintainValue,forkBeltMaintainVal:this.maintainStates[e][1].maintainValue})));return t}},data(){return{loading:!1,settingValueDialogShow:!1,dialog:{title:"",agvName:"",item:100,oriValue:0},itemDescriptionsOfReset:{100:"走行馬達當前里程",101:"Fork皮帶當前里程"},itemDescriptionsOfSetting:{100:"走行馬達保養里程",101:"Fork皮帶保養里程"},maintainStates:{AGV_001:[{VehicleMaintainId:"AGV_001-HORIZON_MOTOR",AGV_Name:"AGV_001",MaintainItem:100,MaintainItemName:"走行馬達保養",currentValue:1,maintainValue:10,VehicleState:null},{VehicleMaintainId:"AGV_001-HORIZON_MOTOR",AGV_Name:"AGV_001",MaintainItem:100,MaintainItemName:"走行馬達保養",currentValue:2,maintainValue:20,VehicleState:null}],AGV_002:[{VehicleMaintainId:"AGV_001-HORIZON_MOTOR",AGV_Name:"AGV_001",MaintainItem:100,MaintainItemName:"走行馬達保養",currentValue:0,maintainValue:0,VehicleState:null},{VehicleMaintainId:"AGV_001-HORIZON_MOTOR",AGV_Name:"AGV_001",MaintainItem:100,MaintainItemName:"走行馬達保養",currentValue:0,maintainValue:0,VehicleState:null}]}}},methods:{async ResetCurrentValue(e,t){const a=this.itemDescriptionsOfReset[t];this.$swal.fire({title:"重置 "+a,text:`確定要重置 ${e} ${a}?`,icon:"question",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((async l=>{if(l.isConfirmed){var i=await j.ResetCurrentValue(e,t);this.getSettings(),i&&q.bM.success({message:`${e} ${a} 重置成功。`,duration:2e3})}}))},ShowSettingDialog(e,t,a){this.dialog.agvName=e,this.dialog.item=t,this.dialog.oriValue=a,this.dialog.title=`${e}-${this.itemDescriptionsOfSetting[t]}設定`,this.settingValueDialogShow=!0},async SettingValue(){var e=await j.SettingMaintainValue(this.dialog.agvName,this.dialog.item,this.dialog.oriValue);this.getSettings(),this.settingValueDialogShow=!1,e?this.$swal.fire({text:"",title:this.dialog.title+"成功",icon:"success",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((e=>{this.settingValueDialogShow=!0})):this.$swal.fire({text:"",title:this.dialog.title+"失敗",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((e=>{this.settingValueDialogShow=!0}))},getSettings(e=!0){this.loading=e,setTimeout((async()=>{this.maintainStates=await j.GetSettings(),this.loading=!1}),200)}},mounted(){this.getSettings(),F.Z.on("Update-Maintain-State",(()=>{this.getSettings(!1)}))}};const Y=(0,p.Z)(Q,[["render",$]]);var J=Y;const X={class:"vehicle-controls bg-light w-100 text-start py-3"};function ee(e,t,a,l,o,n){const r=(0,i.up)("el-button");return(0,i.wg)(),(0,i.iD)("div",X,[(0,i.Wm)(r,{type:"danger",size:"large",onClick:n.HandleShutdownAllVehicleClick},{default:(0,i.w5)((()=>[(0,i.Uk)("Shutdown Vehicles")])),_:1},8,["onClick"])])}var te={methods:{HandleShutdownAllVehicleClick(){this.$swal.fire({text:"",title:"確定要將所有車輛關機?",icon:"warning",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((e=>{e.isConfirmed&&this.ShutDownVehicles()}))},async ShutDownVehicles(){await h.fO.ShutDownAllVehicles()}}};const ae=(0,p.Z)(te,[["render",ee],["__scopeId","data-v-2b7c2414"]]);var le=ae,ie={components:{AddVehicle:f,VehicleMaintain:J,VehicleListTable:M,VehicleControlVue:le},data(){return{test:"AV"}},computed:{isDeveloperUser(){return S.HP.getters.IsDeveloperLogining}}};"function"===typeof l["default"]&&(0,l["default"])(ie);const oe=(0,p.Z)(ie,[["render",d]]);var ne=oe}}]);
//# sourceMappingURL=18.202cf877.js.map