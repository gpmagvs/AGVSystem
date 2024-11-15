"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[544],{92709:function(e,t,a){a.d(t,{LO:function(){return i},ji:function(){return l},uk:function(){return o}});const l={0:"TCP/Socket",1:"RESTFul API"},i={0:{label:"UNKNOWN",labelCN:"未知",color:"rgb(64, 158, 255)"},1:{label:"IDLE",labelCN:"閒置",color:"orange"},2:{label:"RUN",labelCN:"執行",color:"green"},3:{label:"DOWN",labelCN:"當機",color:"red"},4:{label:"Charging",labelCN:"充電",color:"#0d6efd"}},o={0:{label:"FORK",labelCN:"叉車",color:"rgb(64, 158, 255)"},1:{label:"YUNTECH-FORK",labelCN:"叉車(雲科)",color:"orange"},2:{label:"INSPECTOIN",labelCN:"巡檢",color:"green"},3:{label:"SUBMARINE",labelCN:"潛盾",color:"red"},4:{label:"PARTS",labelCN:"Parts",color:"blue"}}},74998:function(e,t,a){a.r(t),a.d(t,{default:function(){return we}});var l={};a.r(l);var i=a(66252);const o={class:"vehicles"},n={class:""},r={class:""},s={class:""};function d(e,t,a,l,d,u){const c=(0,i.up)("VehicleListTable"),m=(0,i.up)("VehicleControlVue"),w=(0,i.up)("b-tab"),h=(0,i.up)("VehicleMaintain"),V=(0,i.up)("AddVehicle"),p=(0,i.up)("b-tabs"),f=(0,i.up)("b-card");return(0,i.wg)(),(0,i.iD)("div",o,[(0,i.Wm)(f,{"no-body":""},{default:(0,i.w5)((()=>[(0,i.Wm)(p,{modelValue:d.activedTab,"onUpdate:modelValue":t[0]||(t[0]=e=>d.activedTab=e),pills:"",vertical:"",justified:"","nav-class":"my-nav","content-class":"my-nav-tabs"},{default:(0,i.w5)((()=>[(0,i.Wm)(w,{title:"車輛列表"},{default:(0,i.w5)((()=>[(0,i._)("div",n,[(0,i.Wm)(c),u.isDeveloperUser?((0,i.wg)(),(0,i.j4)(m,{key:0})):(0,i.kq)("",!0)])])),_:1}),(0,i.Wm)(w,{title:"維修保養"},{default:(0,i.w5)((()=>[(0,i._)("div",r,[(0,i.Wm)(h)])])),_:1}),(0,i.Wm)(w,{title:"新增車輛"},{default:(0,i.w5)((()=>[(0,i._)("div",s,[(0,i.Wm)(V)])])),_:1})])),_:1},8,["modelValue"])])),_:1})])}var u=a(3577);const c={class:"add-vehicle w-100"},m={class:"border-top py-2 text-start"};function w(e,t,a,l,o,n){const r=(0,i.up)("el-divider"),s=(0,i.up)("el-input"),d=(0,i.up)("el-form-item"),w=(0,i.up)("el-option"),h=(0,i.up)("el-select"),V=(0,i.up)("el-switch"),p=(0,i.up)("el-form"),f=(0,i.up)("b-button");return(0,i.wg)(),(0,i.iD)("div",c,[(0,i.Wm)(p,{"label-width":"100px","label-position":"left"},{default:(0,i.w5)((()=>[(0,i.Wm)(r,null,{default:(0,i.w5)((()=>[(0,i.Uk)("Basic")])),_:1}),(0,i.Wm)(d,{label:"車輛ID"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{class:"add-vehicle-input",modelValue:o.payload.AGV_Name,"onUpdate:modelValue":t[0]||(t[0]=e=>o.payload.AGV_Name=e)},null,8,["modelValue"])])),_:1}),(0,i.Wm)(d,{label:"車輛顯示名稱"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{class:"add-vehicle-input"})])),_:1}),(0,i.Wm)(d,{label:"車輛類型"},{default:(0,i.w5)((()=>[(0,i.Wm)(h,{class:"add-vehicle-input",modelValue:o.payload.Model,"onUpdate:modelValue":t[1]||(t[1]=e=>o.payload.Model=e)},{default:(0,i.w5)((()=>[(0,i.Wm)(w,{label:"叉車 AGV",value:0}),(0,i.Wm)(w,{label:"巡檢 AGV",value:2}),(0,i.Wm)(w,{label:"潛盾 AGV",value:3}),(0,i.Wm)(w,{label:"Parts AGV",value:4})])),_:1},8,["modelValue"])])),_:1}),(0,i.Wm)(d,{label:"通訊方式"},{default:(0,i.w5)((()=>[(0,i.Wm)(h,{class:"add-vehicle-input",modelValue:o.payload.Protocol,"onUpdate:modelValue":t[2]||(t[2]=e=>o.payload.Protocol=e)},{default:(0,i.w5)((()=>[(0,i.Wm)(w,{label:"TCP/Socket",value:0}),(0,i.Wm)(w,{label:"RESTFul API",value:1})])),_:1},8,["modelValue"])])),_:1}),(0,i.Wm)(d,{label:"IP"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{class:"add-vehicle-input",modelValue:o.payload.IP,"onUpdate:modelValue":t[3]||(t[3]=e=>o.payload.IP=e)},null,8,["modelValue"])])),_:1}),(0,i.Wm)(d,{label:"Port"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{class:"add-vehicle-input",modelValue:o.payload.Port,"onUpdate:modelValue":t[4]||(t[4]=e=>o.payload.Port=e)},null,8,["modelValue"])])),_:1}),(0,i.Wm)(r,null,{default:(0,i.w5)((()=>[(0,i.Uk)("Layout")])),_:1}),(0,i.Wm)(d,{label:"車輛長度(cm)"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{class:"add-vehicle-input",modelValue:o.payload.VehicleLength,"onUpdate:modelValue":t[5]||(t[5]=e=>o.payload.VehicleLength=e)},null,8,["modelValue"])])),_:1}),(0,i.Wm)(d,{label:"車輛寬度(cm)"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{class:"add-vehicle-input",modelValue:o.payload.VehicleWidth,"onUpdate:modelValue":t[6]||(t[6]=e=>o.payload.VehicleWidth=e)},null,8,["modelValue"])])),_:1}),(0,i.Wm)(r,null,{default:(0,i.w5)((()=>[(0,i.Uk)("Developer")])),_:1}),(0,i.Wm)(d,{label:"模擬"},{default:(0,i.w5)((()=>[(0,i.Wm)(V,{class:"add-vehicle-input",modelValue:o.payload.Simulation,"onUpdate:modelValue":t[7]||(t[7]=e=>o.payload.Simulation=e)},null,8,["modelValue"])])),_:1})])),_:1}),(0,i._)("div",m,[(0,i.Wm)(f,{onClick:t[8]||(t[8]=e=>n.IsEditMode?n.EditVehicle():n.AddVehicle()),variant:"primary",loading:o.adding,style:{width:"120px"}},{default:(0,i.w5)((()=>[(0,i.Uk)((0,u.zw)(n.btnText),1)])),_:1},8,["loading"])])])}var h=a(38418),V={props:{mode:{type:String,default:"add"}},computed:{IsEditMode(){return"edit"==this.mode},btnText(){return this.IsEditMode?"修改":"新增"}},data(){return{payload:{AGV_Name:"AGV_",Model:3,Protocol:0,IP:"127.0.0.1",Port:7025,VehicleLength:145,VehicleWidth:70,Simulation:!1,MaintainSettings:[]},oriAGVID:"",adding:!1}},methods:{async AddVehicle(){this.adding=!0;var e=await h.fO.AddVehicle(this.payload);this.adding=!1,e.confirm?this.$swal.fire({text:"",title:"新增成功",icon:"success",showCancelButton:!1,confirmButtonText:"OK",customClass:"top-most-sweetalert"}):this.$swal.fire({text:e.message,title:"新增失敗",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"top-most-sweetalert"})},async EditVehicle(){var e=await h.fO.EditVehicle(this.payload,this.oriAGVID);e.confirm?this.$swal.fire({text:"",title:"修改成功",icon:"success",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}):this.$swal.fire({text:e.message,title:"修改失敗",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"})},UpdatePayload(e){this.payload=e,this.oriAGVID=e.AGV_Name}}},p=a(83744);const f=(0,p.Z)(V,[["render",w],["__scopeId","data-v-e2d9caf4"]]);var g=f,_=a(49963);const b={class:"vehicle-list-table"},v={class:"text-start"},y={class:""};function S(e,t,a,l,o,n){const r=(0,i.up)("el-skeleton"),s=(0,i.up)("el-table-column"),d=(0,i.up)("el-tag"),c=(0,i.up)("el-checkbox"),m=(0,i.up)("el-button"),w=(0,i.up)("el-table"),h=(0,i.up)("AddVehicle"),V=(0,i.up)("el-drawer"),p=(0,i.up)("VechicleTrafficControlSettings");return(0,i.wg)(),(0,i.iD)("div",b,[(0,i.Wm)(r,{loading:!o.showTable,animated:""},null,8,["loading"]),o.showTable?((0,i.wg)(),(0,i.j4)(w,{key:0,"header-cell-class-name":"my-el-table-cell-class","row-key":"AGV_Name",border:"","header-cell-style":n.tableHeaderStyle,data:n.GetAGVStatesData,size:"large"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{label:"AGV ID",prop:"AGV_Name",width:"130",align:"center"}),(0,i.Wm)(s,{label:"類型",prop:"Model",width:"90",align:"center"},{default:(0,i.w5)((e=>[(0,i.Wm)(d,{effect:"dark"},{default:(0,i.w5)((()=>[(0,i.Uk)((0,u.zw)(n.VehicleModels[e.row.Model].labelCN),1)])),_:2},1024)])),_:1}),(0,i.Wm)(s,{label:"當前狀態",prop:"MainStatus",width:"110",align:"center"},{default:(0,i.w5)((e=>[e.row.Connected?((0,i.wg)(),(0,i.j4)(d,{key:1,effect:"dark",color:n.AGVMainStatus[e.row.MainStatus].color},{default:(0,i.w5)((()=>[(0,i.Uk)((0,u.zw)(n.AGVMainStatus[e.row.MainStatus].label),1)])),_:2},1032,["color"])):((0,i.wg)(),(0,i.j4)(d,{key:0,effect:"dark",type:"danger"},{default:(0,i.w5)((()=>[(0,i.Uk)("斷線")])),_:1}))])),_:1}),(0,i.Wm)(s,{label:"當前位置",prop:"CurrentLocation",align:"center",width:"100"}),(0,i.Wm)(s,{label:"通訊方式",prop:"Protocol",width:"120",align:"center"},{default:(0,i.w5)((e=>[(0,i.Wm)(d,null,{default:(0,i.w5)((()=>[(0,i.Uk)((0,u.zw)(n.ProtocolText[e.row.Protocol]),1)])),_:2},1024)])),_:1}),(0,i.Wm)(s,{label:"IP",prop:"IP",width:"150",align:"center"}),(0,i.Wm)(s,{label:"PORT",prop:"Port",width:"90"}),(0,i.Wm)(s,{label:"車長(cm)",prop:"VehicleLength",width:"100",align:"center"}),(0,i.Wm)(s,{label:"車寬(cm)",prop:"VehicleWidth",width:"100",align:"center"}),(0,i.Wm)(s,{label:"版本號",prop:"AppVersion",width:"120",align:"center"},{default:(0,i.w5)((e=>[(0,i.Wm)(d,null,{default:(0,i.w5)((()=>[(0,i.Uk)((0,u.zw)(e.row.AppVersion),1)])),_:2},1024)])),_:1}),(0,i.Wm)(s,{label:"啟用模擬",prop:"Simulation",width:"100",align:"center"},{default:(0,i.w5)((e=>[(0,i.Wm)(c,{disabled:!0,modelValue:e.row.Simulation,"onUpdate:modelValue":t=>e.row.Simulation=t},null,8,["modelValue","onUpdate:modelValue"])])),_:1}),(0,i.Wm)(s,{fixed:"right",label:"Operations","min-width":"160"},{default:(0,i.w5)((e=>[(0,i.Wm)(m,{size:"small",onClick:(0,_.iM)((t=>n.edit_row(e.row)),["prevent"])},{default:(0,i.w5)((()=>[(0,i.Uk)("編輯")])),_:2},1032,["onClick"]),(0,i.Wm)(m,{size:"small",onClick:(0,_.iM)((t=>n.show_traffic_control_settings_drawer(e.row)),["prevent"])},{default:(0,i.w5)((()=>[(0,i.Uk)("交管設置")])),_:2},1032,["onClick"]),(0,i.Wm)(m,{type:"danger",size:"small",onClick:(0,_.iM)((t=>n.delete_row(e.row)),["prevent"])},{default:(0,i.w5)((()=>[(0,i.Uk)("刪除")])),_:2},1032,["onClick"])])),_:1})])),_:1},8,["header-cell-style","data"])):(0,i.kq)("",!0),(0,i.Wm)(V,{"z-index":1,modelValue:o.ShowEditAGVPropertyDrawer,"onUpdate:modelValue":t[0]||(t[0]=e=>o.ShowEditAGVPropertyDrawer=e)},{header:(0,i.w5)((({})=>[(0,i._)("h3",v,(0,u.zw)(n.drawerText),1)])),default:(0,i.w5)((()=>[(0,i._)("div",y,[(0,i.Wm)(h,{ref:"AgvPropertyEditor",mode:"edit"},null,512)])])),_:1},8,["modelValue"]),(0,i.Wm)(V,{title:"交管設置","append-to-body":"","z-index":1,modelValue:o.ShowVechicleTrafficControlSettingsDrawer,"onUpdate:modelValue":t[1]||(t[1]=e=>o.ShowVechicleTrafficControlSettingsDrawer=e)},{default:(0,i.w5)((()=>[(0,i.Wm)(p,{agv_row:o.selectAGVProertyToEdit},null,8,["agv_row"])])),_:1},8,["modelValue"])])}var W=a(25044),C=a(56938);const k={class:"rounded"},A={class:"d-flex"};var M={__name:"VechicleTrafficControlSettings",props:{agv_row:{type:C.Z,default(){return new C.Z}}},setup(e){return(t,a)=>{const l=(0,i.up)("el-option"),o=(0,i.up)("el-select"),n=(0,i.up)("el-button"),r=(0,i.up)("el-form-item"),s=(0,i.up)("el-form");return(0,i.wg)(),(0,i.iD)("div",k,[(0,i._)("pre",null,(0,u.zw)(e.agv_row),1),(0,i.Wm)(s,{model:e.agv_row},{default:(0,i.w5)((()=>[(0,i.Wm)(r,{label:"變更位置"},{default:(0,i.w5)((()=>[(0,i._)("div",A,[(0,i.Wm)(o,{modelValue:e.agv_row.AGV_Name,"onUpdate:modelValue":a[0]||(a[0]=t=>e.agv_row.AGV_Name=t),placeholder:"請選擇位置"},{default:(0,i.w5)((()=>[(0,i.Wm)(l)])),_:1},8,["modelValue"]),(0,i.Wm)(n,{type:"primary"},{default:(0,i.w5)((()=>[(0,i.Uk)("變更")])),_:1})])])),_:1})])),_:1},8,["model"])])}}};const G=M;var T=G,x=a(92709),D={components:{AddVehicle:g,VechicleTrafficControlSettings:T},inject:["tableHeaderStyle"],data(){return{table:[],showTable:!1,selectAGVProertyToEdit:{},ShowEditAGVPropertyDrawer:!1,ShowVechicleTrafficControlSettingsDrawer:!1}},computed:{GetAGVStatesData(){return W.sn.getters.AGVStatesData},ProtocolText(){return x.ji},AGVMainStatus(){return x.LO},VehicleModels(){return x.uk},drawerText(){return this.selectAGVProertyToEdit.AGV_Name}},methods:{edit_row(e){this.selectAGVProertyToEdit=e,this.ShowEditAGVPropertyDrawer=!0,setTimeout((()=>{this.$refs["AgvPropertyEditor"].UpdatePayload(e)}),1)},async delete_row(e){var t=async()=>{var t=await h.fO.DeleteVehicle(e.AGV_Name);t.confirm?this.$swal.fire({text:"",title:"刪除車輛成功",icon:"success",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}):this.$swal.fire({text:t.message,title:"刪除車輛失敗",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"})};this.$swal.fire({text:"",title:`確定要刪除車輛-${e.AGV_Name}?`,icon:"warning",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((e=>{e.isConfirmed&&t()}))},show_traffic_control_settings_drawer(e){this.selectAGVProertyToEdit=e,this.ShowVechicleTrafficControlSettingsDrawer=!0}},mounted(){setTimeout((()=>{this.showTable=!0}),1e3)}};const N=(0,p.Z)(D,[["render",S],["__scopeId","data-v-1c0291e8"]]);var U=N;const O={class:"w-100"},P={class:"border-bottom my-2 py-1 text-start"},I={class:"w-100 d-flex value-display"},B={class:"flex-fill text-center"},z={class:"w-100 d-flex value-display"},$={class:"flex-fill text-center"},R={class:"w-100 d-flex value-display"},E={class:"flex-fill text-center"},H={class:"w-100 d-flex value-display"},K={class:"flex-fill text-center"},L={class:"d-flex w-100"};function Z(e,t,a,l,o,n){const r=(0,i.up)("el-button"),s=(0,i.up)("el-table-column"),d=(0,i.up)("el-table"),c=(0,i.up)("el-input-number"),m=(0,i.up)("el-dialog"),w=(0,i.Q2)("loading");return(0,i.wg)(),(0,i.iD)("div",O,[(0,i._)("div",P,[(0,i.Wm)(r,{onClick:t[0]||(t[0]=e=>n.getSettings())},{default:(0,i.w5)((()=>[(0,i.Uk)("重新整理")])),_:1})]),(0,i.wy)(((0,i.wg)(),(0,i.j4)(d,{data:n.tableData,"header-cell-class-name":"my-el-table-cell-class","row-key":"agv_name",border:"","header-cell-style":n.tableHeaderStyle,size:"large"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{label:"AGV ID",prop:"agv_name"}),(0,i.Wm)(s,{label:"走行馬達"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{label:"目前累計里程",prop:"currentHorizonMotorVal"},{default:(0,i.w5)((e=>[(0,i._)("div",I,[(0,i._)("div",B,(0,u.zw)(e.row.currentHorizonMotorVal.toFixed(3))+" km",1),(0,i.Wm)(r,{onClick:t=>n.ResetCurrentValue(e.row.agv_name,100)},{default:(0,i.w5)((()=>[(0,i.Uk)("重設")])),_:2},1032,["onClick"])])])),_:1}),(0,i.Wm)(s,{label:"保養里程",prop:"horizonMotorMaintainVal"},{default:(0,i.w5)((e=>[(0,i._)("div",z,[(0,i._)("div",$,(0,u.zw)(e.row.horizonMotorMaintainVal),1),(0,i.Wm)(r,{onClick:t=>n.ShowSettingDialog(e.row.agv_name,100,e.row.horizonMotorMaintainVal)},{default:(0,i.w5)((()=>[(0,i.Uk)("設定")])),_:2},1032,["onClick"])])])),_:1}),(0,i.Wm)(s,{label:"前次保養時間"}),(0,i.Wm)(s,{label:"前次保養人員"})])),_:1}),(0,i.Wm)(s,{label:"牙叉皮帶"},{default:(0,i.w5)((()=>[(0,i.Wm)(s,{label:"目前累計里程",prop:"currentForkBeltVal"},{default:(0,i.w5)((e=>[(0,i._)("div",R,[(0,i._)("div",E,(0,u.zw)(e.row.currentForkBeltVal),1),(0,i.Wm)(r,{onClick:t=>n.ResetCurrentValue(e.row.agv_name,101)},{default:(0,i.w5)((()=>[(0,i.Uk)("重設")])),_:2},1032,["onClick"])])])),_:1}),(0,i.Wm)(s,{label:"保養里程",prop:"forkBeltMaintainVal"},{default:(0,i.w5)((e=>[(0,i._)("div",H,[(0,i._)("div",K,(0,u.zw)(e.row.forkBeltMaintainVal),1),(0,i.Wm)(r,{onClick:t=>n.ShowSettingDialog(e.row.agv_name,101,e.row.forkBeltMaintainVal)},{default:(0,i.w5)((()=>[(0,i.Uk)("設定")])),_:2},1032,["onClick"])])])),_:1}),(0,i.Wm)(s,{label:"前次保養時間"}),(0,i.Wm)(s,{label:"前次保養人員"}),(0,i.Wm)(s,{label:""},{default:(0,i.w5)((()=>[(0,i._)("template",null,[(0,i.Wm)(r,null,{default:(0,i.w5)((()=>[(0,i.Uk)("確認")])),_:1})])])),_:1})])),_:1})])),_:1},8,["data","header-cell-style"])),[[w,o.loading]]),(0,i.Wm)(m,{title:o.dialog.title,modelValue:o.settingValueDialogShow,"onUpdate:modelValue":t[4]||(t[4]=e=>o.settingValueDialogShow=e),draggable:"",modal:!1,width:"400"},{default:(0,i.w5)((()=>[(0,i._)("div",L,[(0,i.Wm)(c,{class:"mx-2",size:"large",modelValue:o.dialog.oriValue,"onUpdate:modelValue":t[1]||(t[1]=e=>o.dialog.oriValue=e)},null,8,["modelValue"]),(0,i.Wm)(r,{size:"large",type:"primary",onClick:t[2]||(t[2]=e=>n.SettingValue())},{default:(0,i.w5)((()=>[(0,i.Uk)("修改")])),_:1}),(0,i.Wm)(r,{size:"large",onClick:t[3]||(t[3]=()=>{o.settingValueDialogShow=!1})},{default:(0,i.w5)((()=>[(0,i.Uk)("取消")])),_:1})])])),_:1},8,["title","modelValue"])])}var j=a(9669),F=a.n(j),q=a(663),Y=F().create({baseURL:q.Z.vms_host});const Q={GetSettings:"api/VehicleMaintain",ResetCurrentVal:"api/VehicleMaintain/ResetCurrentValue",SettingMaintainValue:"api/VehicleMaintain/SettingMaintainValue"},J={GetSettings:async()=>{var e=await Y.get(`${Q.GetSettings}`);return e.data},ResetCurrentValue:async(e="AGV_001",t=100)=>{var a=await Y.post(`${Q.ResetCurrentVal}?agvName=${e}&item=${t}`);return a.data},SettingMaintainValue:async(e="AGV_001",t=100,a=0)=>{var l=await Y.post(`${Q.SettingMaintainValue}?agvName=${e}&item=${t}&value=${a}`);return l.data}};var X=J,ee=a(53259),te=a(49996),ae={inject:["tableHeaderStyle"],computed:{tableData(){var e=Object.keys(this.maintainStates),t=e.map((e=>({agv_name:e,currentHorizonMotorVal:this.maintainStates[e][0].currentValue,horizonMotorMaintainVal:this.maintainStates[e][0].maintainValue,currentForkBeltVal:this.maintainStates[e][1].currentValue,forkBeltMaintainVal:this.maintainStates[e][1].maintainValue,forkBeltMaintainVal:this.maintainStates[e][1].maintainValue})));return t}},data(){return{loading:!1,settingValueDialogShow:!1,dialog:{title:"",agvName:"",item:100,oriValue:0},itemDescriptionsOfReset:{100:"走行馬達當前里程",101:"Fork皮帶當前里程"},itemDescriptionsOfSetting:{100:"走行馬達保養里程",101:"Fork皮帶保養里程"},maintainStates:{AGV_001:[{VehicleMaintainId:"AGV_001-HORIZON_MOTOR",AGV_Name:"AGV_001",MaintainItem:100,MaintainItemName:"走行馬達保養",currentValue:1,maintainValue:10,VehicleState:null},{VehicleMaintainId:"AGV_001-HORIZON_MOTOR",AGV_Name:"AGV_001",MaintainItem:100,MaintainItemName:"走行馬達保養",currentValue:2,maintainValue:20,VehicleState:null}],AGV_002:[{VehicleMaintainId:"AGV_001-HORIZON_MOTOR",AGV_Name:"AGV_001",MaintainItem:100,MaintainItemName:"走行馬達保養",currentValue:0,maintainValue:0,VehicleState:null},{VehicleMaintainId:"AGV_001-HORIZON_MOTOR",AGV_Name:"AGV_001",MaintainItem:100,MaintainItemName:"走行馬達保養",currentValue:0,maintainValue:0,VehicleState:null}]}}},methods:{async ResetCurrentValue(e,t){const a=this.itemDescriptionsOfReset[t];this.$swal.fire({title:"重置 "+a,text:`確定要重置 ${e} ${a}?`,icon:"question",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((async l=>{if(l.isConfirmed){var i=await X.ResetCurrentValue(e,t);this.getSettings(),i&&te.bM.success({message:`${e} ${a} 重置成功。`,duration:2e3})}}))},ShowSettingDialog(e,t,a){this.dialog.agvName=e,this.dialog.item=t,this.dialog.oriValue=a,this.dialog.title=`${e}-${this.itemDescriptionsOfSetting[t]}設定`,this.settingValueDialogShow=!0},async SettingValue(){var e=await X.SettingMaintainValue(this.dialog.agvName,this.dialog.item,this.dialog.oriValue);this.getSettings(),this.settingValueDialogShow=!1,e?this.$swal.fire({text:"",title:this.dialog.title+"成功",icon:"success",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((e=>{this.settingValueDialogShow=!0})):this.$swal.fire({text:"",title:this.dialog.title+"失敗",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((e=>{this.settingValueDialogShow=!0}))},getSettings(e=!0){this.loading=e,setTimeout((async()=>{this.maintainStates=await X.GetSettings(),this.loading=!1}),200)}},mounted(){this.getSettings(),ee.Z.on("Update-Maintain-State",(()=>{this.getSettings(!1)}))}};const le=(0,p.Z)(ae,[["render",Z]]);var ie=le;const oe={class:"vehicle-controls bg-light w-100 text-start py-3"};function ne(e,t,a,l,o,n){const r=(0,i.up)("el-button");return(0,i.wg)(),(0,i.iD)("div",oe,[(0,i.Wm)(r,{type:"danger",size:"large",onClick:n.HandleShutdownAllVehicleClick},{default:(0,i.w5)((()=>[(0,i.Uk)("Shutdown Vehicles")])),_:1},8,["onClick"])])}var re={methods:{HandleShutdownAllVehicleClick(){this.$swal.fire({text:"",title:"確定要將所有車輛關機?",icon:"warning",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((e=>{e.isConfirmed&&this.ShutDownVehicles()}))},async ShutDownVehicles(){await h.fO.ShutDownAllVehicles()}}};const se=(0,p.Z)(re,[["render",ne],["__scopeId","data-v-2b7c2414"]]);var de=se,ue=a(22201),ce={components:{AddVehicle:g,VehicleMaintain:ie,VehicleListTable:U,VehicleControlVue:de},data(){return{test:"AV",activedTab:0}},computed:{isDeveloperUser(){return W.HP.getters.IsDeveloperLogining}},methods:{determineActivedTab(){const e=this.$route.query["tab"];e&&(this.activedTab=e)}},mounted(){this.determineActivedTab();const e=(0,ue.yj)();(0,i.YP)((()=>e.path),((e,t)=>{e==this.$route.path&&this.determineActivedTab()}))}};"function"===typeof l["default"]&&(0,l["default"])(ce);const me=(0,p.Z)(ce,[["render",d]]);var we=me}}]);
//# sourceMappingURL=544.586334b8.js.map