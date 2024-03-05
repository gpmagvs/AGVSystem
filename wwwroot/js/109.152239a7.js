"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[109],{20109:function(e,t,a){a.r(t),a.d(t,{default:function(){return q}});var l=a(79003);const o=e=>((0,l.pushScopeId)("data-v-3529092c"),e=e(),(0,l.popScopeId)(),e),n={class:"equipment-manager border-start border-end"},d={class:"text-start px-1"},i={class:"d-flex"},s={class:"w-100 d-flex flex-row"},r={class:"w-100"},c={class:"hs-signals d-flex"},_=o((()=>(0,l.createElementVNode)("div",{class:"mx-3"},"交握訊號-EQ",-1))),m={class:"hs-signals d-flex"},u=o((()=>(0,l.createElementVNode)("div",{class:"mx-3"},"交握訊號-AGV",-1)));function V(e,t,a,o,V,p){const C=(0,l.resolveComponent)("b-button"),N=(0,l.resolveComponent)("el-table-column"),h=(0,l.resolveComponent)("b-form-input"),E=(0,l.resolveComponent)("RegionsSelector"),w=(0,l.resolveComponent)("el-option"),g=(0,l.resolveComponent)("el-select"),f=(0,l.resolveComponent)("el-button"),q=(0,l.resolveComponent)("el-checkbox"),S=(0,l.resolveComponent)("el-table"),v=(0,l.resolveComponent)("el-form-item"),D=(0,l.resolveComponent)("el-input"),x=(0,l.resolveComponent)("el-form"),b=(0,l.resolveComponent)("el-drawer");return(0,l.openBlock)(),(0,l.createElementBlock)("div",n,[(0,l.createElementVNode)("p",d,[(0,l.createVNode)(C,{variant:"primary",squared:"",onClick:p.SaveSettingHandler},{default:(0,l.withCtx)((()=>[(0,l.createTextVNode)("儲存設定")])),_:1},8,["onClick"]),(0,l.createVNode)(C,{variant:"info",squared:"",class:"mx-2",onClick:p.AddNewEqHandler},{default:(0,l.withCtx)((()=>[(0,l.createTextVNode)("新增設備")])),_:1},8,["onClick"]),(0,l.createVNode)(C,{squared:"",onClick:p.ReloadSettingsHandler},{default:(0,l.withCtx)((()=>[(0,l.createTextVNode)("重新載入")])),_:1},8,["onClick"])]),(0,l.createVNode)(S,{"header-cell-style":{color:"white",backgroundColor:"rgb(13, 110, 253)",fontSize:"12px"},data:V.EqDatas,"row-key":"TagID",size:"small",border:"",height:"680","table-layout":"fixed",style:{width:"1800px"}},{default:(0,l.withCtx)((()=>[(0,l.createVNode)(N,{label:"Index",prop:"index",width:"80",align:"center",fixed:"left"}),(0,l.createVNode)(N,{label:"設備名稱",prop:"Name",width:"250",fixed:"left"},{default:(0,l.withCtx)((e=>[(0,l.createElementVNode)("div",i,[(0,l.createVNode)(h,{state:V.ValidName(e.row),modelValue:e.row.Name,"onUpdate:modelValue":t=>e.row.Name=t,placeholder:"設備名稱",style:{width:"120px"},"no-wheel":!0,size:"sm",min:1,onInput:t=>p.HandleEqNameChange(e.row,e.row.Name)},null,8,["state","modelValue","onUpdate:modelValue","onInput"]),(0,l.createVNode)(C,{class:"mx-1",size:"sm",variant:"primary",onClick:t=>p.HandleUseMapDataDisplayName(e.row.TagID)},{default:(0,l.withCtx)((()=>[(0,l.createTextVNode)("使用圖資設定")])),_:2},1032,["onClick"])])])),_:1}),(0,l.createVNode)(N,{label:"Tag ID",prop:"TagID",width:"100",align:"center",fixed:"left"},{default:(0,l.withCtx)((e=>[(0,l.createVNode)(h,{type:"number",state:V.ValidTag(e.row),modelValue:e.row.TagID,"onUpdate:modelValue":t=>e.row.TagID=t,modelModifiers:{number:!0},placeholder:"tag id","no-wheel":!0,size:"sm",min:1},null,8,["state","modelValue","onUpdate:modelValue"])])),_:1}),(0,l.createVNode)(N,{label:"區域",prop:"Region",width:"130"},{default:(0,l.withCtx)((e=>[(0,l.createVNode)(E,{modelValue:e.row.Region,"onUpdate:modelValue":t=>e.row.Region=t},null,8,["modelValue","onUpdate:modelValue"])])),_:1}),(0,l.createVNode)(N,{label:"下游設備",width:"610"},{default:(0,l.withCtx)((e=>[(0,l.createElementVNode)("div",s,[(0,l.createVNode)(g,{size:"small",modelValue:e.row.ValidDownStreamEndPointNames,"onUpdate:modelValue":t=>e.row.ValidDownStreamEndPointNames=t,multiple:"",placeholder:"Select",style:{width:"1000px"}},{default:(0,l.withCtx)((()=>[((0,l.openBlock)(!0),(0,l.createElementBlock)(l.Fragment,null,(0,l.renderList)(p.GetAvaluableEqNameList(e.row.Name),(e=>((0,l.openBlock)(),(0,l.createBlock)(w,{key:e,label:e,value:e},null,8,["label","value"])))),128))])),_:2},1032,["modelValue","onUpdate:modelValue"]),(0,l.createVNode)(f,{size:"small",type:"default",onClick:()=>{e.row.ValidDownStreamEndPointNames=p.GetAvaluableEqNameList(e.row.Name)}},{default:(0,l.withCtx)((()=>[(0,l.createTextVNode)("使用所有設備")])),_:2},1032,["onClick"]),(0,l.createVNode)(f,{size:"small",type:"danger",onClick:()=>{e.row.ValidDownStreamEndPointNames=[]}},{default:(0,l.withCtx)((()=>[(0,l.createTextVNode)("清除")])),_:2},1032,["onClick"])])])),_:1}),(0,l.createVNode)(N,{label:"允入車款",width:"200"},{default:(0,l.withCtx)((e=>[(0,l.createVNode)(g,{size:"small",modelValue:e.row.Accept_AGV_Type,"onUpdate:modelValue":t=>e.row.Accept_AGV_Type=t},{default:(0,l.withCtx)((()=>[(0,l.createVNode)(w,{value:0,label:"0-不限"}),(0,l.createVNode)(w,{value:1,label:"1-叉車AGV"}),(0,l.createVNode)(w,{value:2,label:"2-潛盾AGV"})])),_:2},1032,["modelValue","onUpdate:modelValue"])])),_:1}),(0,l.createVNode)(N,{label:"空框/實框 訊號檢查",width:"100",align:"center"},{default:(0,l.withCtx)((e=>[(0,l.createVNode)(q,{modelValue:e.row.RackCapcityCheck,"onUpdate:modelValue":t=>e.row.RackCapcityCheck=t},null,8,["modelValue","onUpdate:modelValue"])])),_:1}),(0,l.createVNode)(N,{label:"操作","min-width":"170",fixed:"right"},{default:(0,l.withCtx)((e=>[(0,l.createElementVNode)("div",null,[(0,l.createVNode)(f,{size:"small",type:"default",onClick:t=>p.ConnectionSettingBtnHandle(e.row)},{default:(0,l.withCtx)((()=>[(0,l.createTextVNode)("連線設定")])),_:2},1032,["onClick"]),(0,l.createVNode)(f,{size:"small",type:"danger",onClick:t=>p.RemoveHandle(e.row)},{default:(0,l.withCtx)((()=>[(0,l.createTextVNode)("移除")])),_:2},1032,["onClick"])])])),_:1})])),_:1},8,["header-cell-style","data"]),(0,l.createVNode)(b,{modelValue:V.connection_setting_drawer,"onUpdate:modelValue":t[5]||(t[5]=e=>V.connection_setting_drawer=e),title:`${V.selected_eq.Name}-連線設定`},{default:(0,l.withCtx)((()=>[(0,l.createElementVNode)("div",r,[(0,l.createVNode)(x,{"label-position":"left","label-width":"100"},{default:(0,l.withCtx)((()=>[(0,l.createVNode)(v,{label:"通訊方式"},{default:(0,l.withCtx)((()=>[(0,l.createVNode)(g,{modelValue:V.selected_eq.ConnOptions.ConnMethod,"onUpdate:modelValue":t[0]||(t[0]=e=>V.selected_eq.ConnOptions.ConnMethod=e)},{default:(0,l.withCtx)((()=>[(0,l.createVNode)(w,{label:"Modbus TCP",value:0}),(0,l.createVNode)(w,{label:"Modbus RTU",value:1}),(0,l.createVNode)(w,{label:"TCP/IP",value:2}),(0,l.createVNode)(w,{label:"Serial Port",value:3})])),_:1},8,["modelValue"])])),_:1}),(0,l.createVNode)(v,{label:"IP"},{default:(0,l.withCtx)((()=>[(0,l.createVNode)(D,{disabled:1==V.selected_eq.ConnOptions.ConnMethod,modelValue:V.selected_eq.ConnOptions.IP,"onUpdate:modelValue":t[1]||(t[1]=e=>V.selected_eq.ConnOptions.IP=e)},null,8,["disabled","modelValue"])])),_:1}),(0,l.createVNode)(v,{label:"PORT"},{default:(0,l.withCtx)((()=>[(0,l.createVNode)(D,{disabled:1==V.selected_eq.ConnOptions.ConnMethod,modelValue:V.selected_eq.ConnOptions.Port,"onUpdate:modelValue":t[2]||(t[2]=e=>V.selected_eq.ConnOptions.Port=e),modelModifiers:{number:!0}},null,8,["disabled","modelValue"])])),_:1}),(0,l.createVNode)(v,{label:"COMPORT"},{default:(0,l.withCtx)((()=>[(0,l.createVNode)(D,{disabled:0==V.selected_eq.ConnOptions.ConnMethod,modelValue:V.selected_eq.ConnOptions.ComPort,"onUpdate:modelValue":t[3]||(t[3]=e=>V.selected_eq.ConnOptions.ComPort=e)},null,8,["disabled","modelValue"])])),_:1})])),_:1}),(0,l.createVNode)(f,{type:"default",onClick:t[4]||(t[4]=e=>p.ConnectTestHandle(V.selected_eq))},{default:(0,l.withCtx)((()=>[(0,l.createTextVNode)("通訊測試")])),_:1})])])),_:1},8,["modelValue","title"]),(0,l.createVNode)(b,{modelValue:V.io_check_drawer,"onUpdate:modelValue":t[19]||(t[19]=e=>V.io_check_drawer=e),direction:"btt"},{default:(0,l.withCtx)((()=>[(0,l.createElementVNode)("div",c,[_,(0,l.createElementVNode)("div",{class:"di-status",onClick:t[6]||(t[6]=t=>e.HandleHSsignaleChange(p.selected_eq_io_data.EQName,"L_REQ",!e.scope.row.HS_EQ_L_REQ)),style:(0,l.normalizeStyle)(e.signalOn(p.selected_eq_io_data.HS_EQ_L_REQ))},"L_REQ",4),(0,l.createElementVNode)("div",{class:"di-status",onClick:t[7]||(t[7]=t=>e.HandleHSsignaleChange(p.selected_eq_io_data.EQName,"U_REQ",!e.scope.row.HS_EQ_U_REQ)),style:(0,l.normalizeStyle)(e.signalOn(p.selected_eq_io_data.HS_EQ_U_REQ))},"U_REQ",4),(0,l.createElementVNode)("div",{class:"di-status",onClick:t[8]||(t[8]=t=>e.HandleHSsignaleChange(p.selected_eq_io_data.EQName,"READY",!e.scope.row.HS_EQ_READY)),style:(0,l.normalizeStyle)(e.signalOn(p.selected_eq_io_data.HS_EQ_READY))},"READY",4),(0,l.createElementVNode)("div",{class:"di-status",onClick:t[9]||(t[9]=t=>e.HandleHSsignaleChange(p.selected_eq_io_data.EQName,"UP_READY",!e.scope.row.HS_EQ_UP_READY)),style:(0,l.normalizeStyle)(e.signalOn(p.selected_eq_io_data.HS_EQ_UP_READY))},"UP_READY",4),(0,l.createElementVNode)("div",{class:"di-status",onClick:t[10]||(t[10]=t=>e.HandleHSsignaleChange(p.selected_eq_io_data.EQName,"LOW_READY",!e.scope.row.HS_EQ_LOW_READY)),style:(0,l.normalizeStyle)(e.signalOn(p.selected_eq_io_data.HS_EQ_LOW_READY))},"LOW_READY",4),(0,l.createCommentVNode)("",!0)]),(0,l.createElementVNode)("div",m,[u,(0,l.createElementVNode)("div",{class:"di-status",onClick:t[12]||(t[12]=t=>e.HandleAGVHSSignaleChange(p.selected_eq_io_data.EQName,"To_EQ_Up",!e.scope.row.To_EQ_Up)),style:(0,l.normalizeStyle)(e.signalOn(p.selected_eq_io_data.To_EQ_Up))},"To_EQ_Up",4),(0,l.createElementVNode)("div",{class:"di-status",onClick:t[13]||(t[13]=t=>e.HandleAGVHSSignaleChange(p.selected_eq_io_data.EQName,"To_EQ_Low",!e.scope.row.To_EQ_Low)),style:(0,l.normalizeStyle)(e.signalOn(p.selected_eq_io_data.To_EQ_Low))},"To_EQ_Low",4),(0,l.createElementVNode)("div",{class:"di-status",onClick:t[14]||(t[14]=t=>e.HandleAGVHSSignaleChange(p.selected_eq_io_data.EQName,"VALID",!e.scope.row.HS_AGV_VALID)),style:(0,l.normalizeStyle)(e.signalOn(p.selected_eq_io_data.HS_AGV_VALID))},"VALID",4),(0,l.createElementVNode)("div",{class:"di-status",onClick:t[15]||(t[15]=t=>e.HandleAGVHSSignaleChange(p.selected_eq_io_data.EQName,"TR_REQ",!e.scope.row.HS_AGV_TR_REQ)),style:(0,l.normalizeStyle)(e.signalOn(p.selected_eq_io_data.HS_AGV_TR_REQ))},"TR_REQ",4),(0,l.createElementVNode)("div",{class:"di-status",onClick:t[16]||(t[16]=t=>e.HandleAGVHSSignaleChange(p.selected_eq_io_data.EQName,"BUSY",!e.scope.row.HS_AGV_BUSY)),style:(0,l.normalizeStyle)(e.signalOn(p.selected_eq_io_data.HS_AGV_BUSY))},"BUSY",4),(0,l.createCommentVNode)("",!0),(0,l.createElementVNode)("div",{class:"di-status",onClick:t[18]||(t[18]=t=>e.HandleAGVHSSignaleChange(p.selected_eq_io_data.EQName,"COMPT",!e.scope.row.HS_AGV_COMPT)),style:(0,l.normalizeStyle)(e.signalOn(p.selected_eq_io_data.HS_AGV_COMPT))},"COMPT",4)])])),_:1},8,["modelValue"])])}a(57658);var p=a(64491),C=a(86445),N=a(95320),h=a(24239),E=a(26666),w=(a(36797),{components:{RegionsSelector:C.Z},data(){return{cell_item_size:"",io_check_drawer:!1,connection_setting_drawer:!1,EqDatas:[],EqDatas_Orignal:[],ValidTag:e=>{var t=e.TagID,a=this.EqDatas.filter((t=>t.Name!=e.Name)),l=a.filter((e=>e.TagID==t));return 0==l.length&&t>=1},ValidName:e=>{var t=e.Name,a=this.EqDatas.filter((t=>t.Name==e.Name));return 1==a.length&&""!=t},selected_eq:{}}},methods:{GetAvaluableEqNameList(e){var t=this.EqNames.filter((t=>t!=e));return["ALL",...t]},async SaveSettingHandler(){var e=await(0,p.wW)(this.EqDatas);e.confirm?this.$swal.fire({title:"儲存成功",icon:"success",timer:2e3}):this.$swal.fire({title:"參數設定有誤",text:e.message,icon:"error"})},async DownloadEQOptions(){this.EqDatas=[];var e=h.jU.getters.EqOptions;for(let t=0;t<e.length;t++){const a=e[t];a.index=t,this.EqDatas.push(a)}this.CloneEQDatas()},CloneEQDatas(){this.EqDatas_Orignal=JSON.parse(JSON.stringify(this.EqDatas))},ReloadSettingsHandler(){this.DownloadEQOptions()},AddNewEqHandler(){this.EqDatas.push({index:this.EqDatas.length,Name:`New_EQ_${this.EqDatas.length}`,TagID:1,ConnOptions:{ConnMethod:0,IP:"127.0.0.1",Port:502,ComPort:"COM1"}})},RemoveHandle(e){var t=this.EqDatas.filter((t=>t.Name!=e.Name));this.EqDatas=t},async IOCheckBtnHandle(e){this.selected_eq=e,this.io_check_drawer=!0},async ConnectionSettingBtnHandle(e){this.selected_eq=e,this.connection_setting_drawer=!0},async ConnectTestHandle(e){var t=await(0,p.qN)(e.ConnOptions),a="";a=0==e.ConnOptions.ConnMethod?`Modbus TCP - ${e.ConnOptions.IP}:${e.ConnOptions.Port}`:`Modbus RTU - ${e.ConnOptions.ComPort}`,t.Connected?this.$swal.fire({title:"OK",text:a,icon:"success"}):this.$swal.fire({title:"Fail",text:a,icon:"error"})},beforeRouteLeave(e,t,a){alert("leave!")},async HandleUseMapDataDisplayName(e){var t=await N.p.dispatch("GetMapPointByTag",e);if(t){var a=this.EqDatas.find((t=>t.TagID==e)),l=t.Graph.Display;a.Name=l,this.HandleEqNameChange(a,l),(0,E.bM)({message:`Get Display Name From Map Success(Tag ${e} = ${l})`,duration:1e3,type:"success",title:"設備同步名稱"})}else(0,E.bM)({message:"Get Display Name From Map Fail",duration:1e3,type:"error",title:"設備同步名稱失敗"})},HandleEqNameChange(e,t){var a=e.TagID,l=this.EqDatas_Orignal.find((e=>e.TagID==a));if(l){var o=l.Name,n=this.EqDatas.filter((e=>e.ValidDownStreamEndPointNames.includes(o)));n.forEach((e=>{var a=e.ValidDownStreamEndPointNames.indexOf(o);e.ValidDownStreamEndPointNames[a]=t})),this.CloneEQDatas()}}},mounted(){setTimeout((()=>{this.DownloadEQOptions()}),1e3)},computed:{EqNames(){return this.EqDatas.map((e=>e.Name))},eq_data(){return h.jU.getters.EQData},selected_eq_io_data(){return this.eq_data.find((e=>e.EQName==this.selected_eq.EQName))}}}),g=a(40089);const f=(0,g.Z)(w,[["render",V],["__scopeId","data-v-3529092c"]]);var q=f}}]);
//# sourceMappingURL=109.152239a7.js.map