"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[275],{44275:function(e,t,o){o.r(t),o.d(t,{default:function(){return Ue}});var a=o(66252),s=o(3577);const r={class:"rack-status-view custom-tabs-head p-1"},i={class:"display-mode-container text-start border-bottom my-1 py-1"},l={class:"px-2 text-primary"},n={class:"d-flex flex-row"},c={class:"flex-fill p-2"},u={class:"d-flex flex-row flex-no-wrap w-100 overflow-auto"},d={key:1,style:{height:"100vh",overflow:"auto"}},p={class:"bg-dark text-light"},m={class:"d-flex flex-row"},f={class:"flex-fill"},w={class:"d-flex flex-row flex-no-wrap overflow-auto"};function _(e,t,o,_,y,g){const h=(0,a.up)("el-button"),k=(0,a.up)("el-option"),C=(0,a.up)("el-select"),v=(0,a.up)("el-badge"),b=(0,a.up)("el-progress"),x=(0,a.up)("RackStatus"),P=(0,a.up)("b-tab"),D=(0,a.up)("b-tabs"),I=(0,a.up)("ZoneLowLevelNotifySetting"),S=(0,a.up)("el-drawer");return(0,a.wg)(),(0,a.iD)("div",r,[(0,a._)("div",i,[(0,a.Wm)(h,{loading:y.waitingClearAllNotCargoButHasIDPorts,link:"",type:"primary",disabled:!g.Permission,onClick:g.HandleClearAllNotCargoButHasIDPorts},{default:(0,a.w5)((()=>[(0,a.Uk)("清除所有無料帳籍")])),_:1},8,["loading","disabled","onClick"]),(0,a.Wm)(h,{link:"",type:"primary",disabled:!g.Permission,onClick:t[0]||(t[0]=e=>y.showLowLevelSettingDrawer=!0)},{default:(0,a.w5)((()=>[(0,a.Uk)("低水位提醒設置")])),_:1},8,["disabled"]),(0,a._)("span",l,[(0,a._)("b",null,(0,s.zw)(e.$t("Display Mode")),1)]),(0,a.Wm)(C,{class:"mx-2",modelValue:y.display,"onUpdate:modelValue":t[1]||(t[1]=e=>y.display=e),style:{width:"120px"}},{default:(0,a.w5)((()=>[(0,a.Wm)(k,{label:e.$t("Tabs"),value:"tabs"},null,8,["label"]),(0,a.Wm)(k,{label:e.$t("Single Page"),value:"div"},null,8,["label"])])),_:1},8,["modelValue"])]),"tabs"==y.display?((0,a.wg)(),(0,a.j4)(D,{key:0},{default:(0,a.w5)((()=>[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(g.GroupedWipData,(t=>((0,a.wg)(),(0,a.j4)(P,{key:t.zoneName},{title:(0,a.w5)((()=>[(0,a._)("span",null,(0,s.zw)(t.zoneName),1),(0,a.Wm)(v,{class:"mx-2",type:"warning",value:g.IsHasDataButNoCargo(t.zoneName)?"!":""},null,8,["value"])])),default:(0,a.w5)((()=>[(0,a._)("div",n,[(0,a._)("b",null,(0,s.zw)(e.$t("Rack.Cargo_Spaces")),1),(0,a._)("div",c,[(0,a.Wm)(b,{"stroke-width":18,percentage:t.level,"text-inside":"",striped:"","striped-flow":"",duration:500},{default:(0,a.w5)((()=>[(0,a._)("span",null,(0,s.zw)(t.hasCstPortNum)+"/"+(0,s.zw)(t.totalPorts),1)])),_:2},1032,["percentage"])])]),(0,a._)("div",u,[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(t.zones,(e=>((0,a.wg)(),(0,a.iD)("div",{key:e.WIPName},[(0,a.Wm)(x,{rack_info:e,showLevel:!1},null,8,["rack_info"])])))),128))])])),_:2},1024)))),128))])),_:1})):((0,a.wg)(),(0,a.iD)("div",d,[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(g.GroupedWipData,(t=>((0,a.wg)(),(0,a.iD)("div",{key:t.zoneName,class:"my-2 border"},[(0,a._)("h3",p,(0,s.zw)(t.zoneName),1),(0,a._)("div",m,[(0,a._)("b",null,(0,s.zw)(e.$t("Rack.Cargo_Spaces")),1),(0,a._)("div",f,[(0,a.Wm)(b,{"stroke-width":18,percentage:t.level,"text-inside":"",striped:"","striped-flow":"",duration:500},{default:(0,a.w5)((()=>[(0,a._)("span",null,(0,s.zw)(t.hasCstPortNum)+"/"+(0,s.zw)(t.totalPorts),1)])),_:2},1032,["percentage"])])]),(0,a._)("div",w,[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(t.zones,(e=>((0,a.wg)(),(0,a.iD)("div",{key:e.WIPName},[(0,a.Wm)(x,{rack_info:e,showLevel:!1},null,8,["rack_info"])])))),128))])])))),128))])),(0,a.Wm)(S,{modelValue:y.showLowLevelSettingDrawer,"onUpdate:modelValue":t[2]||(t[2]=e=>y.showLowLevelSettingDrawer=e),title:"低水位提醒設置",size:"50%"},{default:(0,a.w5)((()=>[(0,a.Wm)(I,{opened:y.showLowLevelSettingDrawer},null,8,["opened"])])),_:1},8,["modelValue"])])}const y={class:"rack-status m-0 p-1"},g={class:"my-1 d-flex flex-row bg-light"},h={class:"w-50 text-start px-2"},k={key:0,class:"p-1"},C={key:1,class:"flex-fill p-2"},v={class:"ports-container"};function b(e,t,o,r,i,l){const n=(0,a.up)("el-progress"),c=(0,a.up)("RackPort");return(0,a.wg)(),(0,a.iD)("div",y,[(0,a._)("div",g,[(0,a._)("h3",h,(0,s.zw)(o.rack_info.WIPName),1),o.showLevel?((0,a.wg)(),(0,a.iD)("div",k,[(0,a._)("b",null,(0,s.zw)(e.$t("Rack.Cargo_Spaces")),1)])):(0,a.kq)("",!0),o.showLevel?((0,a.wg)(),(0,a.iD)("div",C,[(0,a.Wm)(n,{"stroke-width":18,percentage:l.Level,"text-inside":"",striped:"","striped-flow":"",duration:40},{default:(0,a.w5)((()=>[(0,a._)("span",null,(0,s.zw)(this.HasCstPortNum)+"/"+(0,s.zw)(this.TotalPorts),1)])),_:1},8,["percentage"])])):(0,a.kq)("",!0)]),(0,a._)("div",v,[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(l.RowsArray(o.rack_info.Rows),(e=>((0,a.wg)(),(0,a.iD)("div",{class:"d-flex flex-row",key:"row-"+e},[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(o.rack_info.Columns,(t=>((0,a.wg)(),(0,a.iD)("div",{class:"d-flex flex-column",key:"col-"+t},[(0,a.Wm)(c,{rack_name:o.rack_info.WIPName,port_info:l.GetPortByColRow(t-1,e-1),IsOvenAsRacks:o.rack_info.IsOvenAsRacks},null,8,["rack_name","port_info","IsOvenAsRacks"])])))),128))])))),128))])])}o(57658);var x=o(49963);const P=e=>((0,a.dD)("data-v-86bb1b88"),e=e(),(0,a.Cn)(),e),D={class:"bg-light border-bottom d-flex py-1"},I={class:"flex-fill text-start px-1"},S={class:"text-danger bg-light w-100 text-start",style:{"max-height":"0",position:"relative",left:"3px",top:"0px"}},R=P((()=>(0,a._)("i",{class:"bi bi-exclamation"},null,-1))),T={key:0,class:"px-2"},N={key:0,class:"bi bi-slash-circle text-danger mx-2"},E={key:0},W={class:"item"},z={class:"title"},$={class:"values d-flex"},U={key:0,class:"item"},H={class:"title"},A={class:"values d-flex"},O={key:1,class:"item"},B={class:"title"},V={class:"values d-flex"},L={key:2,class:"item"},Z={class:"title"},j={class:"values d-flex"},F={key:3,class:"item"},q={class:"title"},M={class:"values"},K={key:4,class:"item"},Y={class:"title"},Q={class:"values"},G={class:"item"},J={class:"title"},X={class:"values"},ee={key:6,class:"item"},te={class:"title"},oe={class:"values"},ae={key:1,class:"h-100"},se=P((()=>(0,a._)("div",{class:"w-100 h-100 align-items-center justify-content-center d-flex"},"NOT RACK PORT",-1))),re=[se],ie={class:"dialog-footer"};function le(e,t,o,r,i,l){const n=(0,a.up)("el-button"),c=(0,a.up)("el-tooltip"),u=(0,a.up)("el-switch"),d=(0,a.up)("el-tag"),p=(0,a.up)("el-radio-button"),m=(0,a.up)("el-radio-group"),f=(0,a.up)("el-input"),w=(0,a.up)("el-form-item"),_=(0,a.up)("el-form"),y=(0,a.up)("el-dialog");return(0,a.wg)(),(0,a.iD)("div",{class:(0,s.C_)(["rack-port",[l.ProductQualityClassName,l.UsableStateClass,l.NotRackPortClass]])},[(0,a._)("div",D,[(0,a._)("span",I,[(0,a.Wm)(c,{disabled:!l.IsDeveloperLogining,placement:"right",effect:"light"},{content:(0,a.w5)((()=>[(0,a._)("div",null,[(0,a.Wm)(n,{onClick:l.HandleRenamePortNoClicked},{default:(0,a.w5)((()=>[(0,a.Uk)((0,s.zw)(e.$t("Rename")),1)])),_:1},8,["onClick"])])])),default:(0,a.w5)((()=>[(0,a._)("label",{class:(0,s.C_)(l.IsInstallEQ?"port-no-display-not-rack":"port-no-display")},(0,s.zw)(l.PortNameDisplay),3)])),_:1},8,["disabled"])]),(0,a.wy)((0,a._)("div",S,[R,(0,a.Uk)(" "+(0,s.zw)(e.$t("Rack.Sensor_Flash")),1)],512),[[x.F8,l.AnySensorFlash]]),l.IsUserLoginAndPermissionAboveOP&&!l.IsInstallEQ?((0,a.wg)(),(0,a.iD)("div",T,[(0,a.Wm)(u,{"active-text":e.$t("Enable"),"inactive-text":e.$t("Disable"),"active-value":1,"inactive-value":0,"inactive-color":"rgb(146, 148, 153)",modelValue:o.port_info.Properties.PortUsable,"onUpdate:modelValue":t[0]||(t[0]=e=>o.port_info.Properties.PortUsable=e),"before-change":l.HandlePortUsableSwitchClicked},null,8,["active-text","inactive-text","modelValue","before-change"])])):(0,a.kq)("",!0),(0,a.Wm)(c,{content:e.$t("PortDisabled"),placement:"top"},{default:(0,a.w5)((()=>[0!=o.port_info.Properties.PortUsable||l.IsUserLoginAndPermissionAboveOP?(0,a.kq)("",!0):((0,a.wg)(),(0,a.iD)("i",N))])),_:1},8,["content"]),(0,a._)("div",{class:"action-buttons justify-content-center",style:(0,s.j5)({width:l.IsUserLoginAndPermissionAboveOP?"45%":"100%"})},[l.IsCarrierIDExist&&l.IsCarrierExist?((0,a.wg)(),(0,a.j4)(n,{key:0,ref:"modify_btn",onClick:l.CstIDEditHandle,type:"success"},{default:(0,a.w5)((()=>[(0,a.Uk)((0,s.zw)(e.$t("Rack.Edit_ID")),1)])),_:1},8,["onClick"])):(0,a.kq)("",!0),!l.IsCarrierIDExist&&l.IsCarrierExist?((0,a.wg)(),(0,a.j4)(n,{key:1,onClick:l.CstIDEditHandle,type:"info"},{default:(0,a.w5)((()=>[(0,a.Uk)((0,s.zw)(e.$t("Rack.Creat_ID")),1)])),_:1},8,["onClick"])):(0,a.kq)("",!0),l.IsCarrierIDExist&&!l.IsCarrierExist?((0,a.wg)(),(0,a.j4)(n,{key:2,onClick:l.RemoveCSTID,type:"danger"},{default:(0,a.w5)((()=>[(0,a.Uk)((0,s.zw)(e.$t("Rack.Remove_ID")),1)])),_:1},8,["onClick"])):(0,a.kq)("",!0)],4)]),l.IsInstallEQ?((0,a.wg)(),(0,a.iD)("div",ae,re)):((0,a.wg)(),(0,a.iD)("div",E,[(0,a._)("div",W,[(0,a._)("div",z,(0,s.zw)(e.$t("RackPort.CarrierID")),1),(0,a._)("div",$,[(0,a.Wm)(c,{placement:"top-start",content:e.$t("Rack.copy"),disabled:!o.port_info.CarrierID},{default:(0,a.w5)((()=>[(0,a.Wm)(d,{onClick:t[1]||(t[1]=e=>l.CopyText(o.port_info.CarrierID)),class:"copy-button",size:"large",effect:"dark",type:""==o.port_info.CarrierID?"info":"primary",style:{width:"135px","font-weight":"bold","letter-spacing":"3px",cursor:"pointer"}},{default:(0,a.w5)((()=>[(0,a.Uk)((0,s.zw)(o.port_info.CarrierID),1)])),_:1},8,["type"])])),_:1},8,["content","disabled"])])]),o.IsOvenAsRacks||!o.port_info.Properties.HasTraySensor||2!=o.port_info.Properties.CargoTypeStore&&0!=o.port_info.Properties.CargoTypeStore?(0,a.kq)("",!0):((0,a.wg)(),(0,a.iD)("div",U,[(0,a._)("div",H,(0,s.zw)(e.$t("CarrierExistSensor_Tray")),1),(0,a._)("div",A,[(0,a._)("div",{class:"exist-sensor round my-1",style:(0,s.j5)(l.ExistSensorTray_1?i.ExistSensorOnStyle:i.ExistSensorOFFStyle),onClick:t[2]||(t[2]=e=>l.HandleExistSensorStateClick("tray",0))},null,4),(0,a._)("div",{class:"exist-sensor round my-1 mx-3",style:(0,s.j5)(l.ExistSensorTray_2?i.ExistSensorOnStyle:i.ExistSensorOFFStyle),onClick:t[3]||(t[3]=e=>l.HandleExistSensorStateClick("tray",1))},null,4)])])),o.IsOvenAsRacks||!o.port_info.Properties.HasRackSensor||2!=o.port_info.Properties.CargoTypeStore&&1!=o.port_info.Properties.CargoTypeStore?(0,a.kq)("",!0):((0,a.wg)(),(0,a.iD)("div",O,[(0,a._)("div",B,(0,s.zw)(e.$t("CarrierExistSensor_Rack")),1),(0,a._)("div",V,[(0,a._)("div",{class:"exist-sensor round my-1",style:(0,s.j5)(l.ExistSensorRack_1?i.ExistSensorOnStyle:i.ExistSensorOFFStyle),onClick:t[4]||(t[4]=e=>l.HandleExistSensorStateClick("rack",0))},null,4),(0,a._)("div",{class:"exist-sensor round my-1 mx-3",style:(0,s.j5)(l.ExistSensorRack_2?i.ExistSensorOnStyle:i.ExistSensorOFFStyle),onClick:t[5]||(t[5]=e=>l.HandleExistSensorStateClick("rack",1))},null,4)])])),o.IsOvenAsRacks||!o.port_info.Properties.HasTrayDirectionSensor||2!=o.port_info.Properties.CargoTypeStore&&1!=o.port_info.Properties.CargoTypeStore?(0,a.kq)("",!0):((0,a.wg)(),(0,a.iD)("div",L,[(0,a._)("div",Z,(0,s.zw)(e.$t("TrayDirection")),1),(0,a._)("div",j,[(0,a._)("div",{class:"exist-sensor round my-1",style:(0,s.j5)(l.TrayDirectionSensor?i.ExistSensorOnStyle:i.ExistSensorOFFStyle)},null,4)])])),o.IsOvenAsRacks?((0,a.wg)(),(0,a.iD)("div",F,[(0,a._)("div",q,(0,s.zw)(e.$t("RackPort.CarrierExist")),1),(0,a._)("div",M,[(0,a.Wm)(d,{size:"large",effect:"dark",type:o.port_info.CarrierExist?"success":"danger"},{default:(0,a.w5)((()=>[(0,a.Uk)((0,s.zw)(o.port_info.CarrierExist?e.$t("RackPort.HasCargo"):e.$t("RackPort.NoCargo")),1)])),_:1},8,["type"])])])):(0,a.kq)("",!0),o.IsOvenAsRacks?((0,a.wg)(),(0,a.iD)("div",K,[(0,a._)("div",Y,(0,s.zw)(e.$t("RackPort.EmptyorFillFrame")),1),(0,a._)("div",Q,[(0,a.Wm)(m,{disabled:i.radioGroupDisable,size:"large",modelValue:o.port_info.RackContentState,"onUpdate:modelValue":t[6]||(t[6]=e=>o.port_info.RackContentState=e),fill:"rgb(8, 87, 60)"},{default:(0,a.w5)((()=>[(0,a.Wm)(p,{onClick:l.EmptyContentClick,value:0},{default:(0,a.w5)((()=>[(0,a.Uk)((0,s.zw)(e.$t("RackPort.EmptyFrame")),1)])),_:1},8,["onClick"]),(0,a.Wm)(p,{onClick:l.FullContentClick,value:1},{default:(0,a.w5)((()=>[(0,a.Uk)((0,s.zw)(e.$t("RackPort.FillFrame")),1)])),_:1},8,["onClick"])])),_:1},8,["disabled","modelValue"])])])):(0,a.kq)("",!0),(0,a.kq)("",!0),(0,a._)("div",G,[(0,a._)("div",J,(0,s.zw)(e.$t("RackPort.InstallTime")),1),(0,a._)("div",X,(0,s.zw)(l.InstallTime),1)]),o.port_info.Properties.DisplayRackTypeStoring?((0,a.wg)(),(0,a.iD)("div",ee,[(0,a._)("div",te,[(0,a.Wm)(c,{content:"貨物類型可能為Tray、空框、未烘烤實框、已烘烤實框或未知",placement:"top",effect:"light"},{default:(0,a.w5)((()=>[(0,a.Uk)((0,s.zw)(e.$t("RackPort.RackType")),1)])),_:1})]),(0,a._)("div",oe,[(0,a.Wm)(d,{type:"info",style:{width:"135px"}},{default:(0,a.w5)((()=>[(0,a._)("i",{class:(0,s.C_)(l.RackTypeClass),style:(0,s.j5)(l.RackTypeClassStyle)},null,6),(0,a._)("span",null,(0,s.zw)(l.RackType),1)])),_:1})])])):(0,a.kq)("",!0)])),(0,a.Wm)(y,{modelValue:i.showPortNoRenameDialog,"onUpdate:modelValue":t[11]||(t[11]=e=>i.showPortNoRenameDialog=e),title:`Port No Rename: ${o.port_info.Properties.ID}`,width:"30%",draggable:"","close-on-click-modal":!1,modal:!1},{default:(0,a.w5)((()=>[(0,a.Wm)(_,{"label-position":"top"},{default:(0,a.w5)((()=>[(0,a.Wm)(w,{label:"Current Port No:"},{default:(0,a.w5)((()=>[(0,a.Wm)(f,{modelValue:o.port_info.Properties.PortNo,"onUpdate:modelValue":t[8]||(t[8]=e=>o.port_info.Properties.PortNo=e),disabled:!0},null,8,["modelValue"])])),_:1}),(0,a.Wm)(w,{label:"New Port No:",required:""},{default:(0,a.w5)((()=>[(0,a.Wm)(f,{modelValue:i.newPortNo,"onUpdate:modelValue":t[9]||(t[9]=e=>i.newPortNo=e),placeholder:"Enter new port number",autofocus:"",ref:"portNoInput",rules:[{required:!0,message:"Port number is required"},{pattern:/^[A-Za-z0-9-_]+$/,message:"Only letters, numbers, hyphens and underscores allowed"},{min:1,max:20,message:"Length must be between 1-20 characters"}]},null,8,["modelValue"])])),_:1})])),_:1}),(0,a._)("div",ie,[(0,a.Wm)(n,{onClick:t[10]||(t[10]=e=>i.showPortNoRenameDialog=!1)},{default:(0,a.w5)((()=>[(0,a.Uk)("Cancel")])),_:1}),(0,a.Wm)(n,{type:"primary",onClick:l.handlePortNoRename},{default:(0,a.w5)((()=>[(0,a.Uk)("Confirm")])),_:1},8,["onClick"])])])),_:1},8,["modelValue","title"])],2)}var ne=o(42152),ce=o.n(ne),ue=o(10844),de=o(49996),pe=o(81348),me=o(69215),fe=o(25044),we=o(64491),_e=o(53259),ye=o(30381),ge=o.n(ye),he={props:{rack_name:{type:String,default:""},port_info:{type:Object,default(){return{CargoExist:!1,CarrierID:null,SensorStates:{TRAY_1:!1,TRAY_2:!0,RACK_1:!1,RACK_2:!1,TRAY_DIRECTION:!1},InstallTime:"0001-01-01T00:00:00",Properties:{ID:"0-0",Row:0,Column:0,PortNo:"",PortUsable:0,ProductionQualityStore:0,CargoTypeStore:2,IOLocation:{Tray_Sensor1:0,Tray_Sensor2:1,Box_Sensor1:2,Box_Sensor2:3},StoragePriority:0,HasTrayDirectionSensor:!1,HasTraySensor:!0,HasRackSensor:!0,DisplayRackTypeStoring:!1},RackPlacementState:0,TrayPlacementState:0}}},IsOvenAsRacks:{type:Boolean,default:!1}},data(){return{ExistSensorOnStyle:{backgroundColor:"lime"},ExistSensorOFFStyle:{backgroundColor:"rgb(180, 183, 191)"},selectedRackContentType:"",radioGroupDisable:!1,showPortNoRenameDialog:!1,newPortNo:""}},computed:{IsInstallEQ(){if(!this.port_info.Properties.EQInstall.IsUseForEQ)return!1;const e=this.port_info.Properties.EQInstall.BindingEQName,t=fe.jU.state.EqOptions.find((t=>t.Name==e));return t&&!t.IsRoleAsZone},BindingEQName(){return this.port_info.Properties.EQInstall.BindingEQName},IsUserLoginAndPermissionAboveOP(){return fe.HP.state.user.Role>0},ProductQualityClassName(){return this.IsOvenAsRacks?"oven-port":this.port_info.CargoExist&&this.port_info.CarrierID?"has-cst-port":!this.port_info.CargoExist&&this.port_info.CarrierID?"has-data-but-no-cargo-port":this.port_info.CargoExist&&!this.port_info.CarrierID?"has-cargo-but-no-cst-port":"empty-port"},UsableStateClass(){return!this.port_info||this.port_info.disabledTempotary?"port-not-usable-temportary":1==this.port_info.Properties.PortUsable?"port-usable":"port-not-usable"},NotRackPortClass(){return this.IsInstallEQ?"not-rack-port":""},PortNameDisplay(){return`${this.port_info.PortNo}`},IsCarrierIDExist(){return this.port_info.CarrierID&&""!=this.port_info.CarrierID},IsCarrierExist(){return this.port_info.CargoExist},ModifyButtonText(){return this.IsCarrierIDExist?"修改帳籍":"新增帳籍"},ExistSensorTray_1(){return 0!=this.port_info.MaterialExistSensorStates["TRAY_1"]},ExistSensorTray_2(){return 0!=this.port_info.MaterialExistSensorStates["TRAY_2"]},ExistSensorRack_1(){return 0!=this.port_info.MaterialExistSensorStates["RACK_1"]},ExistSensorRack_2(){return 0!=this.port_info.MaterialExistSensorStates["RACK_2"]},TrayDirectionSensor(){return 0!=this.port_info.SensorStates["TRAY_DIRECTION"]},AnySensorFlash(){var e=Object.values(this.port_info.MaterialExistSensorStates),t=e.filter((e=>2==e));return 0!=t.length},IsDeveloperLogining(){return fe.HP.getters.IsDeveloperLogining},InstallTime(){return this.port_info.CarrierID&&""!=this.port_info.CarrierID?ge()(this.port_info.InstallTime).format("YYYY-MM-DD HH:mm:ss"):""},RackTypeClass(){const e=this.port_info.StoredRackContentType;switch(e){case 0:return"bi bi-box2";case 1:return"bi bi-box-fill";case 2:return"bi bi-box-seam-fill";default:return"bi bi-question-circle"}},RackTypeClassStyle(){const e=this.port_info.StoredRackContentType;let t="rgb(0, 0, 0)",o="1rem",a="8px";switch(e){case 0:t="rgb(0, 0, 0)";break;case 1:t="rgb(138, 213, 255)";break;case 2:t="rgb(255, 0, 0)";break;default:t="rgb(0, 0, 0)";break}return{color:t,fontSize:o,marginRight:a}},RackType(){if(!this.port_info)return"未知";{const e=this.port_info.StoredRackContentType;switch(e){case 0:return"空框";case 1:return"未烘烤實框";case 2:return"已烘烤實框";case 3:return"未知"}}}},methods:{async HandleExistSensorStateClick(e="tray|rack",t=1){if(this.IsDeveloperLogining){var o=await we.k3.SetSensorState(this.rack_name,this.port_info.Properties.ID,e,t,!1);o.confirm||this.$swal.fire({text:"",title:`${o.message}`,icon:"info",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"})}},CstIDEditHandle(){ue.T.prompt("Carrier ID",{title:`${this.ModifyButtonText} : ${this.PortNameDisplay}`,draggable:!0,inputValue:this.port_info.CarrierID,type:"warning",confirmButtonText:"修改",inputErrorMessage:"帳籍ID不得為空",inputPlaceholder:"請輸入ID",inputValidator:e=>""!=e}).then((async e=>{console.info(e);var t=e.value?e.value:"";let o=await(0,me.m7)(this.rack_name,this.port_info.Properties.ID,t);o.confirm?this.$message({message:"帳籍已修改。",type:"success"}):this.$swal.fire({text:o.message,title:"",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"})})).catch((()=>{}))},RemoveCSTID(){this.$swal.fire({text:`確定要移除帳籍-[${this.port_info.CarrierID}] ?`,title:"移除帳籍確認",icon:"question",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((async e=>{if(e.isConfirmed){let e=await(0,me.pz)(this.rack_name,this.port_info.Properties.ID);e.confirm?this.$message({message:"帳籍已移除。"}):this.$swal.fire({text:e.message,title:"",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"})}}))},CopyText(e){if(!e||""==e)return;const t=document.createElement("button");t.className="copy-button",document.body.appendChild(t);const o=new(ce())(".copy-button",{text:()=>e});o.on("success",(()=>{(0,de.bM)({title:e,message:"已複製到剪貼簿",duration:1500}),o.destroy(),document.body.removeChild(t)})),o.on("error",(()=>{(0,de.bM)({title:"錯誤",message:"複製失敗",type:"error",duration:1500}),o.destroy(),document.body.removeChild(t)})),t.click()},async EmptyContentClick(){this.radioGroupDisable=!0,await(0,we.Kz)(this.port_info.TagNumbers[0],!0),await(0,we.hE)(this.port_info.TagNumbers[0],!1),this.radioGroupDisable=!1},async FullContentClick(){this.radioGroupDisable=!0,await(0,we.hE)(this.port_info.TagNumbers[0],!0),await(0,we.Kz)(this.port_info.TagNumbers[0],!1),this.radioGroupDisable=!1},async HandlePortUsableSwitchClicked(){return setTimeout((async()=>{const e=`${this.rack_name}-${this.port_info.PortNo}`,t=1!=this.port_info.Properties.PortUsable,o=await(0,me.Bh)(this.rack_name,this.port_info.Properties.ID,t);o.confirm?(_e.Z.emit("home-reload-request","rack-port-usable-changed"),(0,de.bM)({title:"成功",message:t?`${e}已啟用`:`${e}已停用`,type:"success",duration:1500})):this.$swal.fire({text:o.message,title:`切換${e} ${t?"啟用":"禁用"}失敗`,icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"})}),300),!1},HandleRenamePortNoClicked(){this.showPortNoRenameDialog=!0,setTimeout((()=>{this.$refs.portNoInput.focus()}),100)},async handlePortNoRename(){if(console.log(this.newPortNo),this.newPortNo)if(this.newPortNo.length<1||this.newPortNo.length>20)(0,pe.z8)({message:"編號長度必須在1-20個字元之間",type:"warning",duration:2e3});else try{await(0,me.Vd)(this.rack_name,this.port_info.Properties.ID,this.newPortNo);(0,pe.z8)({message:`Port編號已更新為 ${this.newPortNo}`,type:"success",duration:2e3}),this.showPortNoRenameDialog=!1}catch(e){this.showPortNoRenameDialog=!1,this.$swal.fire({text:e.message,title:"",icon:"error",showCancelButton:!1,confirmButtonText:"OK"}).then((()=>{this.showPortNoRenameDialog=!0}))}else(0,pe.z8)({message:"請輸入新的Port編號",type:"warning",duration:2e3})}}},ke=o(83744);const Ce=(0,ke.Z)(he,[["render",le],["__scopeId","data-v-86bb1b88"]]);var ve=Ce,be={components:{RackPort:ve},methods:{GetPortByColRow(e,t){var o=this.rack_info.Ports;return o.find((o=>o.Properties.Row==t&&o.Properties.Column==e))},RowsArray(e){let t=[];for(var o=e;o>=1;o--)t.push(o);return t}},props:{rack_info:{type:Object,default(){return{WIPName:"Rack-1",Rows:3,Columns:3,ColumnsTagMap:{0:[0],1:[1],2:[2]},Ports:[{CargoExist:!1,CarrierID:null,MaterialExistSensorStates:{TRAY_1:!1,TRAY_2:!0,RACK_1:!1,RACK_2:!1},InstallTime:"0001-01-01T00:00:00",Properties:{ID:"0-0",Row:0,Column:0,IOLocation:{Tray_Sensor1:0,Tray_Sensor2:1,Box_Sensor1:2,Box_Sensor2:3}},RackPlacementState:0,TrayPlacementState:0}]}}},showLevel:{type:Boolean,default:!0}},computed:{TotalPorts(){return this.rack_info.Rows*this.rack_info.Columns},HasCstPortNum(){let e=0;for(let t=0;t<this.rack_info.Ports.length;t++)!0===this.rack_info.Ports[t].CargoExist&&e++;return e},Level(){return this.HasCstPortNum/this.TotalPorts*100}}};const xe=(0,ke.Z)(be,[["render",b],["__scopeId","data-v-56a68298"]]);var Pe=xe,De=o(2262),Ie=o(65781);const Se={class:"d-flex border-bottom pb-2 mb-2"},Re=(0,a._)("span",null,"門檻值",-1),Te=(0,a._)("span",null,"通知訊息模板",-1);var Ne={__name:"ZoneLowLevelNotifySetting",props:{opened:{type:Boolean,default:!1}},setup(e){const t=e,o=(0,De.iH)(!1);var s=(0,De.iH)([]);const r=(0,a.Fl)((()=>[...s.value].sort(((e,t)=>e.ZoneID.localeCompare(t.ZoneID)))));(0,a.YP)((()=>t.opened),(()=>{t.opened&&i()}));const i=()=>{o.value=!0,(0,me.qE)().then((e=>{s.value=e})).finally((()=>{setTimeout((()=>{o.value=!1}),500)}))};(0,a.bv)((()=>{i()}));const l=(e,t)=>e.IsNewAdded?{backgroundColor:"#ffa66a"}:{},n=()=>{ue.T.prompt("請輸入Zone ID","新增Zone設定",{confirmButtonText:"確定",cancelButtonText:"取消",inputPattern:/^[A-Za-z0-9]+$/,inputErrorMessage:"Zone ID只能包含英數字"}).then((({value:e})=>{s.value.some((t=>t.ZoneID===e))?pe.z8.error("此Zone ID已存在"):s.value.push({ZoneID:e,DisplayZoneName:"Name of "+e,ThresHoldValue:0,NotifyMessageTemplate:"可用的Tray盤數即將不足，請補空Tray! 當前數量:{AvailableNumber}",IsNewAdded:!0})}))},c=e=>{const t=r.value[e].ZoneID,o=s.value.findIndex((e=>e.ZoneID===t));-1!==o&&s.value.splice(o,1)},u=()=>{let e={};s.value.forEach((t=>{e[t.ZoneID]={DisplayZoneName:t.DisplayZoneName,ThresHoldValue:t.ThresHoldValue,NotifyMessageTemplate:t.NotifyMessageTemplate}})),(0,me.j5)(e).then((e=>{i(),pe.z8.success("儲存成功")})).catch((e=>{pe.z8.error("儲存失敗")}))};return(e,t)=>{const s=(0,a.up)("el-button"),d=(0,a.up)("el-table-column"),p=(0,a.up)("el-input"),m=(0,a.up)("el-icon"),f=(0,a.up)("el-tooltip"),w=(0,a.up)("el-input-number"),_=(0,a.up)("el-table"),y=(0,a.Q2)("loading");return(0,a.wg)(),(0,a.iD)("div",null,[(0,a._)("div",Se,[(0,a.Wm)(s,{type:"primary",onClick:u},{default:(0,a.w5)((()=>[(0,a.Uk)("儲存")])),_:1}),(0,a.Wm)(s,{onClick:i},{default:(0,a.w5)((()=>[(0,a.Uk)("重新載入")])),_:1}),(0,a.Wm)(s,{type:"info",onClick:n},{default:(0,a.w5)((()=>[(0,a.Uk)("新增")])),_:1})]),(0,a.wy)(((0,a.wg)(),(0,a.j4)(_,{data:r.value,border:"",size:"large","row-style":l},{default:(0,a.w5)((()=>[(0,a.Wm)(d,{label:"ZoneID",prop:"ZoneID",width:"150"}),(0,a.Wm)(d,{label:"顯示名稱",prop:"DisplayZoneName",width:"150"},{default:(0,a.w5)((e=>[(0,a.Wm)(p,{modelValue:e.row.DisplayZoneName,"onUpdate:modelValue":t=>e.row.DisplayZoneName=t},null,8,["modelValue","onUpdate:modelValue"])])),_:1}),(0,a.Wm)(d,{label:"門檻值",width:"190"},{header:(0,a.w5)((()=>[Re,(0,a.Wm)(f,{content:"當可用空Tray數量小於門檻值時，會發送通知訊息。若門檻值為0，效果等同於不啟用提醒。",placement:"top"},{default:(0,a.w5)((()=>[(0,a.Wm)(m,{class:"ms-2"},{default:(0,a.w5)((()=>[(0,a.Wm)((0,De.SU)(Ie.cEj))])),_:1})])),_:1})])),default:(0,a.w5)((e=>[(0,a.Wm)(w,{modelValue:e.row.ThresHoldValue,"onUpdate:modelValue":t=>e.row.ThresHoldValue=t},null,8,["modelValue","onUpdate:modelValue"])])),_:1}),(0,a.Wm)(d,{label:"通知訊息模板"},{header:(0,a.w5)((()=>[Te,(0,a.Wm)(f,{content:"可用變數: {AvailableNumber} - 當前可用數量",placement:"top"},{default:(0,a.w5)((()=>[(0,a.Wm)(m,{class:"ms-2"},{default:(0,a.w5)((()=>[(0,a.Wm)((0,De.SU)(Ie.cEj))])),_:1})])),_:1})])),default:(0,a.w5)((e=>[(0,a.Wm)(p,{modelValue:e.row.NotifyMessageTemplate,"onUpdate:modelValue":t=>e.row.NotifyMessageTemplate=t},null,8,["modelValue","onUpdate:modelValue"])])),_:1}),(0,a.Wm)(d,{label:"操作",width:"100"},{default:(0,a.w5)((e=>[(0,a.Wm)(s,{type:"danger",size:"small",onClick:t=>c(e.$index)},{default:(0,a.w5)((()=>[(0,a.Uk)(" 刪除 ")])),_:2},1032,["onClick"])])),_:1})])),_:1},8,["data"])),[[y,o.value]])])}}};const Ee=Ne;var We=Ee,ze={components:{RackStatus:Pe,ZoneLowLevelNotifySetting:We},data(){return{display:"div",showLowLevelSettingDrawer:!1,waitingClearAllNotCargoButHasIDPorts:!1}},computed:{WIPData(){return fe.jU.state.WIPsData},GroupedWipData(){const e=fe.jU.state.WIPsData;let t=t=>{let s=e.filter((e=>e.DeviceID==t));const r=s.reduce(((e,t)=>e+a(t)),0),i=s.reduce(((e,t)=>e+o(t)),0);return{zoneName:t,zones:s,totalPorts:r,hasCstPortNum:i,level:i/r*100}},o=e=>{let t=0;for(let o=0;o<e.Ports.length;o++)!0===e.Ports[o].CargoExist&&t++;return t},a=e=>e.Rows*e.Columns;const s=[...new Set(e.map((e=>e.DeviceID)))];return s.map((e=>t(e)))},Permission(){return fe.HP.state.user.Role>1}},watch:{display(e){localStorage.setItem("wips-display-mode",e)}},mounted(){var e=localStorage.getItem("wips-display-mode");e&&(this.display=e)},methods:{async HandleClearAllNotCargoButHasIDPorts(){const e=await this.$swal.fire({title:"確認清除",text:"是否要清除所有有帳無料的帳籍?",icon:"warning",showCancelButton:!0,confirmButtonText:"確定",cancelButtonText:"取消"});if(e.isConfirmed){this.waitingClearAllNotCargoButHasIDPorts=!0;const e=await(0,me.IC)();this.$message.success(`有帳無料帳籍清除成功，共${e.total}筆，成功${e.success}筆，失敗${e.fail}筆`),this.waitingClearAllNotCargoButHasIDPorts=!1}},IsHasDataButNoCargo(e){const t=fe.jU.state.HasDataButNoCargoWIPs.find((t=>t.DeviceID==e));return void 0!=t}}};const $e=(0,ke.Z)(ze,[["render",_]]);var Ue=$e}}]);
//# sourceMappingURL=275.9845809b.js.map