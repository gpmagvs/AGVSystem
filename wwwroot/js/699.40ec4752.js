"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[699],{14699:function(e,t,s){s.r(t),s.d(t,{default:function(){return O}});var a=s(66252),l=s(3577);const i=e=>((0,a.dD)("data-v-6f34ef99"),e=e(),(0,a.Cn)(),e),n={class:"user-manager p-2"},o={class:"d-flex flex-row"},r=i((()=>(0,a._)("span",{class:"flex-fill"},null,-1))),d={class:"border"},u=i((()=>(0,a._)("h3",null,"使用者權限設定",-1))),m={class:"user-name"},w={style:{position:"relative",top:"-60px"}};function c(e,t,s,i,c,p){const g=(0,a.up)("OperatieButtonSet"),f=(0,a.up)("el-button"),h=(0,a.up)("el-table-column"),S=(0,a.up)("el-input"),U=(0,a.up)("el-option"),v=(0,a.up)("el-select"),b=(0,a.up)("el-table"),y=(0,a.up)("PermissionSetting"),N=(0,a.up)("el-drawer"),P=(0,a.up)("el-form-item"),_=(0,a.up)("b-button"),C=(0,a.up)("el-form"),k=(0,a.up)("el-dialog");return(0,a.wg)(),(0,a.iD)("div",n,[(0,a._)("div",o,[(0,a.Wm)(g,{onSave:p.SaveSetting,onRestore:p.HandleRestoreBtnClicked},null,8,["onSave","onRestore"]),r,(0,a.Wm)(f,{onClick:t[0]||(t[0]=()=>{c.NewUser={UserName:"",Password:"",Role:1},c.AddNewUserDialogShow=!0}),style:{"font-weight":"bold","font-size":"16px"},type:"primary",size:"large"},{default:(0,a.w5)((()=>[(0,a.Uk)("新增使用者")])),_:1})]),(0,a._)("div",d,[(0,a.Wm)(b,{data:c.UserData,"empty-text":"沒有使用者",size:"large","header-cell-style":c.tableHeaderStyle,border:""},{default:(0,a.w5)((()=>[(0,a.Wm)(h,{label:"使用者名稱",prop:"UserName",width:"180"}),(0,a.Wm)(h,{label:"使用者密碼",prop:"Password",width:"220"},{default:(0,a.w5)((e=>[(0,a._)("div",null,[(0,a.Wm)(S,{type:"Password",modelValue:e.row.Password,"onUpdate:modelValue":t=>e.row.Password=t,"show-password":""},null,8,["modelValue","onUpdate:modelValue"])])])),_:1}),(0,a.Wm)(h,{label:"使用者群組",prop:"Role",width:"180"},{default:(0,a.w5)((e=>[(0,a._)("div",null,[(0,a.Wm)(v,{modelValue:e.row.Role,"onUpdate:modelValue":t=>e.row.Role=t},{default:(0,a.w5)((()=>[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(c.RoleOptions,(e=>((0,a.wg)(),(0,a.j4)(U,{key:e.value,value:e.value,label:e.label},null,8,["value","label"])))),128))])),_:2},1032,["modelValue","onUpdate:modelValue"])])])),_:1}),(0,a.Wm)(h,{label:"操作","min-width":"120"},{default:(0,a.w5)((e=>[(0,a._)("div",null,[(0,a.Wm)(f,{onClick:()=>{c.PermissionSettingSender.userName=e.row.UserName,c.PermissionSettingSender.role=e.row.Role,c.showPermissionSettingDrawer=!0}},{default:(0,a.w5)((()=>[(0,a.Uk)("權限設定")])),_:2},1032,["onClick"])])])),_:1}),(0,a.Wm)(h,{label:"操作","min-width":"120"},{default:(0,a.w5)((e=>[(0,a._)("div",null,[(0,a.Wm)(f,{onClick:t=>p.DeleteUser(e.row),type:"danger"},{default:(0,a.w5)((()=>[(0,a.Uk)("刪除")])),_:2},1032,["onClick"])])])),_:1})])),_:1},8,["data","header-cell-style"])]),(0,a.Wm)(N,{modelValue:c.showPermissionSettingDrawer,"onUpdate:modelValue":t[1]||(t[1]=e=>c.showPermissionSettingDrawer=e),size:600,"custom-class":"permission-drawer"},{header:(0,a.w5)((({titleId:e,titleClass:t})=>[(0,a._)("div",null,[u,(0,a._)("h4",m,"["+(0,l.zw)(c.PermissionSettingSender.userName)+"]",1)])])),default:(0,a.w5)((()=>[(0,a._)("div",w,[(0,a.Wm)(y,{PermissionSettingSender:c.PermissionSettingSender},null,8,["PermissionSettingSender"])])])),_:1},8,["modelValue"]),(0,a.Wm)(k,{modelValue:c.AddNewUserDialogShow,"onUpdate:modelValue":t[5]||(t[5]=e=>c.AddNewUserDialogShow=e),width:"500",title:"新增使用者",draggable:""},{default:(0,a.w5)((()=>[(0,a._)("div",null,[(0,a.Wm)(C,null,{default:(0,a.w5)((()=>[(0,a.Wm)(P,{label:"使用者名稱"},{default:(0,a.w5)((()=>[(0,a.Wm)(S,{modelValue:c.NewUser.UserName,"onUpdate:modelValue":t[2]||(t[2]=e=>c.NewUser.UserName=e),autocomplete:"off"},null,8,["modelValue"])])),_:1}),(0,a.Wm)(P,{label:"使用者密碼"},{default:(0,a.w5)((()=>[(0,a.Wm)(S,{modelValue:c.NewUser.Password,"onUpdate:modelValue":t[3]||(t[3]=e=>c.NewUser.Password=e),type:"password",autocomplete:"off"},null,8,["modelValue"])])),_:1}),(0,a.Wm)(P,{label:"使用者群組"},{default:(0,a.w5)((()=>[(0,a.Wm)(v,{modelValue:c.NewUser.Role,"onUpdate:modelValue":t[4]||(t[4]=e=>c.NewUser.Role=e)},{default:(0,a.w5)((()=>[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(c.RoleOptions,(e=>((0,a.wg)(),(0,a.j4)(U,{key:e.value,value:e.value,label:e.label},null,8,["value","label"])))),128))])),_:1},8,["modelValue"])])),_:1}),(0,a.Wm)(_,{disabled:""==c.NewUser.UserName||""==c.NewUser.Password,onClick:p.HandleAddNewUserClick,class:"w-100",variant:"primary"},{default:(0,a.w5)((()=>[(0,a.Uk)("新增")])),_:1},8,["disabled","onClick"])])),_:1})])])),_:1},8,["modelValue"])])}const p={class:"save-default-buttons-group border-bottom bg-default text-start"};function g(e,t,s,l,i,n){const o=(0,a.up)("el-button");return(0,a.wg)(),(0,a.iD)("div",p,[(0,a.Wm)(o,{onClick:n.HandleSaveBtnClick,type:"primary"},{default:(0,a.w5)((()=>[(0,a.Uk)("儲存")])),_:1},8,["onClick"]),(0,a.Wm)(o,{onClick:n.HandleRestoreBtnClick,type:"danger"},{default:(0,a.w5)((()=>[(0,a.Uk)("預設值")])),_:1},8,["onClick"])])}var f={methods:{HandleRestoreBtnClick(){this.$emit("restore","")},HandleSaveBtnClick(){this.$emit("save","")}}},h=s(83744);const S=(0,h.Z)(f,[["render",g],["__scopeId","data-v-5df3efc3"]]);var U=S;const v={class:"permission-settings"},b={class:"text-start py-2"};function y(e,t,s,i,n,o){const r=(0,a.up)("el-button"),d=(0,a.up)("el-switch"),u=(0,a.up)("el-form-item"),m=(0,a.up)("el-form");return(0,a.wg)(),(0,a.iD)("div",v,[(0,a._)("div",b,[(0,a.Wm)(r,{class:"",type:"primary",onClick:o.savePermissions,loading:n.isSaving},{default:(0,a.w5)((()=>[(0,a.Uk)("儲存權限設置")])),_:1},8,["onClick","loading"])]),(0,a.Wm)(m,{"label-position":"left","label-width":"200px",class:"permission-form"},{default:(0,a.w5)((()=>[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(n.permissionSections,((e,t)=>((0,a.wg)(),(0,a.iD)("div",{key:t,class:"permission-section"},[(0,a._)("h3",null,(0,l.zw)(e.title),1),((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(n.PermissionSetting[t],((e,s)=>((0,a.wg)(),(0,a.j4)(u,{key:s,label:o.translateLabel(s)},{default:(0,a.w5)((()=>[(0,a.Wm)(d,{modelValue:n.PermissionSetting[t][s],"onUpdate:modelValue":e=>n.PermissionSetting[t][s]=e,"active-value":1,"inactive-value":0,"active-color":"#13ce66","inactive-color":"#ff4949"},null,8,["modelValue","onUpdate:modelValue"])])),_:2},1032,["label"])))),128))])))),128))])),_:1})])}var N=s(82106),P=s(90131),_=s(24239),C={props:{PermissionSettingSender:{type:Object,default(){return{userName:"",role:-1}}}},data(){return{PermissionSetting:new P.kg,permissionSections:{menu:{title:"選單權限"},dataQuerySubMenu:{title:"資料查詢子選單權限"},systemConfigurationSubMenu:{title:"系統設定子選單權限"}},isSaving:!1}},watch:{PermissionSettingSender:{handler(e,t){console.log("PermissionSettingSender changed:",e),(0,N.E4)(e.userName).then((e=>{Object.assign(this.PermissionSetting,e)}))},deep:!0,immediate:!0}},methods:{translateLabel(e){const t={SystemAlarm:"系統警報",WIPInfo:"WIP資訊",VehicleManagnment:"車輛管理",Map:"地圖",DataQuery:"資料查詢",HotRun:"HOT RUN",SystemConfiguration:"系統配置",TaskHistory:"任務歷史",AlarmHistory:"警報歷史",VehicleTrajectory:"車輛軌跡查詢",InstrumentsMeasure:"儀器測量查詢",Utilization:"稼動率查詢",BatteryLevelManagnment:"電池電量管理",EquipmentlManagnment:"設備管理",RackManagnment:"RACK管理",UserManagnment:"用戶管理",ChargerManagnment:"充電站管理"};return t[e]||e},async savePermissions(){this.isSaving=!0;try{if(await(0,N.Yo)(this.PermissionSettingSender.userName,this.PermissionSetting),this.$message.success("權限設置已成功儲存"),this.PermissionSettingSender.userName==_.HP.state.user.UserName){var e=JSON.parse(JSON.stringify(this.PermissionSetting));_.HP.commit("updateCurrentUserPermission",e)}}catch(t){console.error("保存權限設置時出錯:",t),this.$message.error("儲存權限設置失敗，請稍後再試")}finally{this.isSaving=!1}}}};const k=(0,h.Z)(C,[["render",y],["__scopeId","data-v-1b743df6"]]);var D=k,W=s(93867),V=s(97318),R={components:{OperatieButtonSet:U,PermissionSetting:D},data(){return{tableHeaderStyle:V.R,AddNewUserDialogShow:!1,showPermissionSettingDrawer:!1,UserData:[{UserName:"jinwei",Password:"1020",Role:2},{UserName:"eng",Password:"tset-pw2",Role:1},{UserName:"test",Password:"tset-pw3",Role:0}],NewUser:{UserName:"eng",Password:"tset-pw2",Role:1},OriginUserData:{},RoleOptions:[{label:"Operator",value:0},{label:"工程師",value:1},{label:"開發人員",value:2}],PermissionSettingSender:{userName:"",role:-1}}},mounted(){this.DownloadUsers()},methods:{async DeleteUser(e){this.$swal.fire({text:`確定要刪除使用者-${e.UserName} ?`,title:"刪除使用者",icon:"question",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((async t=>{t.isConfirmed&&(await(0,N.HG)(e.UserName),this.DownloadUsers(),W.Z.Danger(`使用者-${e.UserName}已刪除`,"top",1e3))}))},async DownloadUsers(){this.UserData=await(0,N.jf)(),this.SaveAsOriginUserData()},async SaveSetting(){var e=await(0,N.l2)(this.UserData);e.Success?W.Z.Success("使用者資料修改成功","bottom",3e3):W.Z.Danger("使用者資料修改失敗","bottom",3e3)},async HandleRestoreBtnClicked(){this.$swal.fire({text:"",title:"確定要載入預設值?",icon:"question",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((e=>{e.isConfirmed&&(this.DownloadUsers(),W.Z.Success("使用者資料已重新載入","top",2e3))}))},SaveAsOriginUserData(){this.OriginUserData=JSON.parse(JSON.stringify(this.UserData))},async HandleAddNewUserClick(){var e=await(0,N.mm)(this.NewUser);this.AddNewUserDialogShow=!1,e.Success?this.$swal.fire({text:"",title:`使用者 ${this.NewUser.UserName}-新增成功`,icon:"success",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}):this.$swal.fire({text:`${e.Message}`,title:"使用者新增失敗",icon:"warning",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((()=>{this.AddNewUserDialogShow=!0})),this.DownloadUsers()}}};const H=(0,h.Z)(R,[["render",c],["__scopeId","data-v-6f34ef99"]]);var O=H}}]);
//# sourceMappingURL=699.40ec4752.js.map