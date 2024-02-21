"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[463],{30594:function(e,t,l){l.r(t),l.d(t,{default:function(){return V}});var a=l(79003);const o={class:"user-manager"},s={class:"d-flex flex-row"},r=(0,a.createElementVNode)("span",{class:"flex-fill"},null,-1),n={class:"border"};function d(e,t,l,d,i,c){const u=(0,a.resolveComponent)("OperatieButtonSet"),w=(0,a.resolveComponent)("el-button"),m=(0,a.resolveComponent)("el-table-column"),p=(0,a.resolveComponent)("el-input"),N=(0,a.resolveComponent)("el-option"),h=(0,a.resolveComponent)("el-select"),C=(0,a.resolveComponent)("el-table"),U=(0,a.resolveComponent)("el-form-item"),V=(0,a.resolveComponent)("b-button"),v=(0,a.resolveComponent)("el-form"),f=(0,a.resolveComponent)("el-dialog");return(0,a.openBlock)(),(0,a.createElementBlock)("div",o,[(0,a.createElementVNode)("div",s,[(0,a.createVNode)(u,{onSave:c.SaveSetting,onRestore:c.HandleRestoreBtnClicked},null,8,["onSave","onRestore"]),r,(0,a.createVNode)(w,{onClick:t[0]||(t[0]=()=>{i.NewUser={UserName:"",Password:"",Role:1},i.AddNewUserDialogShow=!0}),style:{"font-weight":"bold","font-size":"20px"},type:"primary",size:"large"},{default:(0,a.withCtx)((()=>[(0,a.createTextVNode)("新增使用者")])),_:1})]),(0,a.createElementVNode)("div",n,[(0,a.createVNode)(C,{data:i.UserData,"empty-text":"沒有使用者",size:"large"},{default:(0,a.withCtx)((()=>[(0,a.createVNode)(m,{label:"使用者名稱",prop:"UserName",width:"180"}),(0,a.createVNode)(m,{label:"使用者密碼",prop:"Password",width:"220"},{default:(0,a.withCtx)((e=>[(0,a.createElementVNode)("div",null,[(0,a.createVNode)(p,{type:"Password",modelValue:e.row.Password,"onUpdate:modelValue":t=>e.row.Password=t,"show-password":""},null,8,["modelValue","onUpdate:modelValue"])])])),_:1}),(0,a.createVNode)(m,{label:"使用者群組",prop:"Role",width:"180"},{default:(0,a.withCtx)((e=>[(0,a.createElementVNode)("div",null,[(0,a.createVNode)(h,{modelValue:e.row.Role,"onUpdate:modelValue":t=>e.row.Role=t},{default:(0,a.withCtx)((()=>[((0,a.openBlock)(!0),(0,a.createElementBlock)(a.Fragment,null,(0,a.renderList)(i.RoleOptions,(e=>((0,a.openBlock)(),(0,a.createBlock)(N,{key:e.value,value:e.value,label:e.label},null,8,["value","label"])))),128))])),_:2},1032,["modelValue","onUpdate:modelValue"])])])),_:1}),(0,a.createVNode)(m,{label:"操作","min-width":"120"},{default:(0,a.withCtx)((e=>[(0,a.createElementVNode)("div",null,[(0,a.createVNode)(w,{onClick:t=>c.DeleteUser(e.row),type:"danger"},{default:(0,a.withCtx)((()=>[(0,a.createTextVNode)("刪除")])),_:2},1032,["onClick"])])])),_:1})])),_:1},8,["data"])]),(0,a.createVNode)(f,{modelValue:i.AddNewUserDialogShow,"onUpdate:modelValue":t[4]||(t[4]=e=>i.AddNewUserDialogShow=e),width:"500",title:"新增使用者",draggable:""},{default:(0,a.withCtx)((()=>[(0,a.createElementVNode)("div",null,[(0,a.createVNode)(v,null,{default:(0,a.withCtx)((()=>[(0,a.createVNode)(U,{label:"使用者名稱"},{default:(0,a.withCtx)((()=>[(0,a.createVNode)(p,{modelValue:i.NewUser.UserName,"onUpdate:modelValue":t[1]||(t[1]=e=>i.NewUser.UserName=e),autocomplete:"off"},null,8,["modelValue"])])),_:1}),(0,a.createVNode)(U,{label:"使用者密碼"},{default:(0,a.withCtx)((()=>[(0,a.createVNode)(p,{modelValue:i.NewUser.Password,"onUpdate:modelValue":t[2]||(t[2]=e=>i.NewUser.Password=e),type:"password",autocomplete:"off"},null,8,["modelValue"])])),_:1}),(0,a.createVNode)(U,{label:"使用者群組"},{default:(0,a.withCtx)((()=>[(0,a.createVNode)(h,{modelValue:i.NewUser.Role,"onUpdate:modelValue":t[3]||(t[3]=e=>i.NewUser.Role=e)},{default:(0,a.withCtx)((()=>[((0,a.openBlock)(!0),(0,a.createElementBlock)(a.Fragment,null,(0,a.renderList)(i.RoleOptions,(e=>((0,a.openBlock)(),(0,a.createBlock)(N,{key:e.value,value:e.value,label:e.label},null,8,["value","label"])))),128))])),_:1},8,["modelValue"])])),_:1}),(0,a.createVNode)(V,{disabled:""==i.NewUser.UserName||""==i.NewUser.Password,onClick:c.HandleAddNewUserClick,class:"w-100",variant:"primary"},{default:(0,a.withCtx)((()=>[(0,a.createTextVNode)("新增")])),_:1},8,["disabled","onClick"])])),_:1})])])),_:1},8,["modelValue"])])}const i={class:"save-default-buttons-group border-bottom bg-default text-start"};function c(e,t,l,o,s,r){const n=(0,a.resolveComponent)("el-button");return(0,a.openBlock)(),(0,a.createElementBlock)("div",i,[(0,a.createVNode)(n,{onClick:r.HandleSaveBtnClick,type:"primary",size:"large"},{default:(0,a.withCtx)((()=>[(0,a.createTextVNode)("儲存")])),_:1},8,["onClick"]),(0,a.createVNode)(n,{onClick:r.HandleRestoreBtnClick,type:"danger",size:"large"},{default:(0,a.withCtx)((()=>[(0,a.createTextVNode)("預設值")])),_:1},8,["onClick"])])}var u={methods:{HandleRestoreBtnClick(){this.$emit("restore","")},HandleSaveBtnClick(){this.$emit("save","")}}},w=l(40089);const m=(0,w.Z)(u,[["render",c],["__scopeId","data-v-6bebdf16"]]);var p=m,N=l(82106),h=l(93867),C={components:{OperatieButtonSet:p},data(){return{AddNewUserDialogShow:!1,UserData:[{UserName:"jinwei",Password:"1020",Role:2},{UserName:"eng",Password:"tset-pw2",Role:1},{UserName:"test",Password:"tset-pw3",Role:0}],NewUser:{UserName:"eng",Password:"tset-pw2",Role:1},OriginUserData:{},RoleOptions:[{label:"Operator",value:0},{label:"工程師",value:1},{label:"開發人員",value:2}]}},mounted(){this.DownloadUsers()},methods:{async DeleteUser(e){this.$swal.fire({text:`確定要刪除使用者-${e.UserName} ?`,title:"刪除使用者",icon:"question",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((async t=>{t.isConfirmed&&(await(0,N.HG)(e.UserName),this.DownloadUsers(),h.Z.Danger(`使用者-${e.UserName}已刪除`,"top",1e3))}))},async DownloadUsers(){this.UserData=await(0,N.jf)(),this.SaveAsOriginUserData()},async SaveSetting(){var e=await(0,N.l2)(this.UserData);e.Success?h.Z.Success("使用者資料修改成功","bottom",3e3):h.Z.Danger("使用者資料修改失敗","bottom",3e3)},async HandleRestoreBtnClicked(){this.$swal.fire({text:"",title:"確定要載入預設值?",icon:"question",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((e=>{e.isConfirmed&&(this.DownloadUsers(),h.Z.Success("使用者資料已重新載入","top",2e3))}))},SaveAsOriginUserData(){this.OriginUserData=JSON.parse(JSON.stringify(this.UserData))},async HandleAddNewUserClick(){var e=await(0,N.mm)(this.NewUser);this.AddNewUserDialogShow=!1,e.Success?this.$swal.fire({text:"",title:`使用者 ${this.NewUser.UserName}-新增成功`,icon:"success",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}):this.$swal.fire({text:`${e.Message}`,title:"使用者新增失敗",icon:"warning",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((()=>{this.AddNewUserDialogShow=!0})),this.DownloadUsers()}}};const U=(0,w.Z)(C,[["render",d]]);var V=U}}]);
//# sourceMappingURL=463.de28b71c.js.map