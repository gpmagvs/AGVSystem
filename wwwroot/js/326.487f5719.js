"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[326],{37326:function(e,t,l){l.r(t),l.d(t,{default:function(){return W}});l(57658);var a=l(66252),o=l(3577);const n={class:"p-2 text-start"},s={class:"w-100 border-bottom my-1"},i={class:"mx-2"},r=["id"],d={class:"px-2 text-start"},u={class:"text-start w-100"},c={key:0},m={key:1},p={class:"w-100 bg-light px-2 text-start"};function w(e,t,l,w,_,h){const g=(0,a.up)("el-divider"),v=(0,a.up)("el-button"),f=(0,a.up)("el-table-column"),b=(0,a.up)("el-checkbox"),k=(0,a.up)("el-option"),y=(0,a.up)("el-select"),C=(0,a.up)("el-tag"),V=(0,a.up)("el-input-number"),W=(0,a.up)("el-input"),R=(0,a.up)("el-table"),S=(0,a.up)("el-drawer"),U=(0,a.up)("el-form-item"),x=(0,a.up)("el-form");return(0,a.wg)(),(0,a.iD)("div",null,[(0,a.Wm)(g,{"content-position":"left"},{default:(0,a.w5)((()=>[(0,a.Uk)("腳本")])),_:1}),(0,a._)("div",n,[(0,a._)("div",s,[(0,a.Wm)(v,{onClick:h.HandleSaveBtnClick,type:"primary"},{default:(0,a.w5)((()=>[(0,a.Uk)("儲存腳本設定")])),_:1},8,["onClick"]),(0,a.Wm)(v,{class:"my-1",type:"danger",onClick:t[0]||(t[0]=()=>{_.hotRunScripts.push({no:_.hotRunScripts.length+1,agv_name:void 0,loop_num:10,finish_num:0,action_num:9,state:"IDLE",actions:[]})})},{default:(0,a.w5)((()=>[(0,a.Uk)("新增動作")])),_:1})]),(0,a.Wm)(R,{"row-key":"scriptID",data:_.hotRunScripts,"default-expand-all":!1,border:""},{default:(0,a.w5)((()=>[(0,a.Wm)(f,{label:"NO.",prop:"no",width:"60",align:"center"}),(0,a.Wm)(f,{label:"ID",prop:"scriptID",width:"210",align:"center"}),(0,a.Wm)(f,{label:"隨機搬運",prop:"IsRandomCarryRun",align:"center"},{default:(0,a.w5)((e=>[(0,a.Wm)(b,{modelValue:e.row.IsRandomCarryRun,"onUpdate:modelValue":t=>e.row.IsRandomCarryRun=t},null,8,["modelValue","onUpdate:modelValue"])])),_:1}),(0,a.Wm)(f,{label:"執行AGV",prop:"agv_name",width:"150"},{default:(0,a.w5)((e=>[(0,a._)("div",null,[(0,a.Wm)(y,{disabled:e.row.IsRandomCarryRun,modelValue:e.row.agv_name,"onUpdate:modelValue":t=>e.row.agv_name=t},{default:(0,a.w5)((()=>[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(h.AgvNameList,(e=>((0,a.wg)(),(0,a.j4)(k,{key:e,label:e,value:e},null,8,["label","value"])))),128))])),_:2},1032,["disabled","modelValue","onUpdate:modelValue"])])])),_:1}),(0,a.Wm)(f,{label:"狀態",prop:"state",width:"120",align:"center"},{default:(0,a.w5)((e=>[(0,a._)("div",null,[(0,a.Wm)(C,{effect:"dark",type:"Running"==e.row.state?"success":"warning"},{default:(0,a.w5)((()=>[(0,a.Uk)((0,o.zw)(e.row.state),1)])),_:2},1032,["type"])])])),_:1}),(0,a.Wm)(f,{label:"已完成次數",prop:"finish_num",width:"100",align:"center"},{default:(0,a.w5)((e=>[(0,a._)("div",null,(0,o.zw)(e.row.finish_num)+"/"+(0,o.zw)(e.row.loop_num),1)])),_:1}),(0,a.Wm)(f,{label:"Loop次數",prop:"finish_num",width:"120",align:"center"},{default:(0,a.w5)((e=>[(0,a.Wm)(V,{disabled:e.row.IsRandomCarryRun,size:"small",style:{width:"80px"},step:1,precision:0,modelValue:e.row.loop_num,"onUpdate:modelValue":t=>e.row.loop_num=t},null,8,["disabled","modelValue","onUpdate:modelValue"])])),_:1}),(0,a.Wm)(f,{label:"動作數",width:"140"},{default:(0,a.w5)((e=>[(0,a._)("div",null,[(0,a._)("span",i,(0,o.zw)(e.row.actions.length),1),(0,a.Wm)(v,{disabled:e.row.IsRandomCarryRun,size:"small",onClick:()=>{_.action_drawer_visible=!0,_.selected_script_name=e.row.agv_name,_.selected_script_actions=e.row.actions}},{default:(0,a.w5)((()=>[(0,a.Uk)("動作設定")])),_:2},1032,["disabled","onClick"])])])),_:1}),(0,a.Wm)(f,{label:"Comment",width:"240"},{default:(0,a.w5)((e=>[(0,a.Wm)(W,{modelValue:e.row.comment,"onUpdate:modelValue":t=>e.row.comment=t},null,8,["modelValue","onUpdate:modelValue"])])),_:1}),(0,a.Wm)(f,{label:"即時資訊",prop:"RealTimeMessage",width:"auto"}),(0,a.Wm)(f,null,{default:(0,a.w5)((e=>[(0,a._)("div",null,[(0,a.Wm)(v,{type:"Running"==e.row.state?"danger":"success",size:"small",onClick:t=>h.HandleStartBtnClick(e.row)},{default:(0,a.w5)((()=>[(0,a.Uk)((0,o.zw)("Running"==e.row.state?"中止":"執行"),1)])),_:2},1032,["type","onClick"]),(0,a.Wm)(v,{size:"small",onClick:t=>h.HandleDeleteScript(e.row),type:"danger"},{default:(0,a.w5)((()=>[(0,a.Uk)("刪除")])),_:2},1032,["onClick"])])])),_:1})])),_:1},8,["data"]),(0,a.Wm)(S,{modelValue:_.action_drawer_visible,"onUpdate:modelValue":t[2]||(t[2]=e=>_.action_drawer_visible=e),direction:"rtl",size:"60%"},{header:(0,a.w5)((({titleId:e,titleClass:t})=>[(0,a._)("h4",{class:(0,o.C_)(["text-danger px-5 text-center",t]),id:e},"HOT RUN Actions Setting : "+(0,o.zw)(_.selected_script_name),11,r)])),default:(0,a.w5)((()=>[(0,a._)("div",d,[(0,a.Wm)(v,{class:"mx-2",type:"danger",onClick:t[1]||(t[1]=()=>{_.selected_script_actions.push({no:_.selected_script_actions.length+1,action:"move",source_tag:void 0,destine_tag:void 0})})},{default:(0,a.w5)((()=>[(0,a.Uk)("新增動作")])),_:1}),(0,a.Wm)(v,{class:"mx-2",onClick:h.HandleSaveBtnClickInDrawer,type:"primary"},{default:(0,a.w5)((()=>[(0,a.Uk)("儲存設定")])),_:1},8,["onClick"]),(0,a.Wm)(R,{"row-key":"no",style:{width:"1024px"},border:"",class:"m-2",data:_.selected_script_actions},{default:(0,a.w5)((()=>[(0,a.Wm)(f,{width:"50"},{default:(0,a.w5)((e=>[(0,a._)("div",u,[(0,a.Wm)(v,{class:"w-100",size:"small",onClick:t=>h.action_move_up(e.row)},{default:(0,a.w5)((()=>[(0,a.Uk)("▲")])),_:2},1032,["onClick"]),(0,a.Wm)(v,{class:"w-100",size:"small",onClick:t=>h.action_move_down(e.row)},{default:(0,a.w5)((()=>[(0,a.Uk)("▼")])),_:2},1032,["onClick"])])])),_:1}),(0,a.Wm)(f,{label:"NO.",prop:"no",width:"60",align:"center"}),(0,a.Wm)(f,{label:"動作",prop:"action",width:"120"},{default:(0,a.w5)((e=>[(0,a.Wm)(y,{class:"w-100",modelValue:e.row.action,"onUpdate:modelValue":t=>e.row.action=t,placeholder:"請選擇Action"},{default:(0,a.w5)((()=>[(0,a.Wm)(k,{label:"移動",value:"move"}),(0,a.Wm)(k,{label:"停車",value:"park"}),(0,a.Wm)(k,{label:"搬運",value:"carry"}),(0,a.Wm)(k,{label:"放貨",value:"load"}),(0,a.Wm)(k,{label:"取貨",value:"unload"}),(0,a.Wm)(k,{label:"充電",value:"charge"}),(0,a.Wm)(k,{label:"巡檢量測",value:"measure"}),(0,a.Wm)(k,{label:"交換電池",value:"exchangebattery"})])),_:2},1032,["modelValue","onUpdate:modelValue"])])),_:1}),(0,a.Wm)(f,{label:"起點",prop:"source_tag",width:"250"},{default:(0,a.w5)((e=>[(0,a.Wm)(y,{disabled:"carry"!=e.row.action,class:"w-100",modelValue:e.row.source_tag,"onUpdate:modelValue":t=>e.row.source_tag=t,placeholder:"請選擇起點"},{default:(0,a.w5)((()=>[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(h.GetOption(e.row.action),(e=>((0,a.wg)(),(0,a.j4)(k,{key:e,label:e.name,value:e.tag},null,8,["label","value"])))),128))])),_:2},1032,["disabled","modelValue","onUpdate:modelValue"]),h.slotSelectable(e.row.action)?((0,a.wg)(),(0,a.j4)(y,{key:0,disabled:"carry"!=e.row.action,class:"w-100",modelValue:e.row.source_slot,"onUpdate:modelValue":t=>e.row.source_slot=t,placeholder:"請選擇Slot"},{default:(0,a.w5)((()=>[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(h.SlotOptions,(e=>((0,a.wg)(),(0,a.j4)(k,{key:e.value,label:e.label,value:e.value},null,8,["label","value"])))),128))])),_:2},1032,["disabled","modelValue","onUpdate:modelValue"])):(0,a.kq)("",!0)])),_:1}),(0,a.Wm)(f,{label:"卡匣ID",width:"150"},{default:(0,a.w5)((e=>[(0,a.Wm)(W,{disabled:"carry"!=e.row.action&&"unload"!=e.row.action,modelValue:e.row.cst_id,"onUpdate:modelValue":t=>e.row.cst_id=t},null,8,["disabled","modelValue","onUpdate:modelValue"])])),_:1}),(0,a.Wm)(f,{label:"終點",prop:"destine_tag",width:"250"},{default:(0,a.w5)((e=>["measure"!=e.row.action?((0,a.wg)(),(0,a.iD)("div",c,[(0,a.Wm)(y,{class:"w-100",modelValue:e.row.destine_tag,"onUpdate:modelValue":t=>e.row.destine_tag=t,placeholder:"請選擇終點"},{default:(0,a.w5)((()=>[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(h.GetOption(e.row.action),(e=>((0,a.wg)(),(0,a.j4)(k,{key:e,label:e.name,value:e.tag},null,8,["label","value"])))),128))])),_:2},1032,["modelValue","onUpdate:modelValue"]),h.slotSelectable(e.row.action)?((0,a.wg)(),(0,a.j4)(y,{key:0,class:"w-100",modelValue:e.row.destine_slot,"onUpdate:modelValue":t=>e.row.destine_slot=t,placeholder:"請選擇Slot"},{default:(0,a.w5)((()=>[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(h.SlotOptions,(e=>((0,a.wg)(),(0,a.j4)(k,{key:e.value,label:e.label,value:e.value},null,8,["label","value"])))),128))])),_:2},1032,["modelValue","onUpdate:modelValue"])):(0,a.kq)("",!0)])):((0,a.wg)(),(0,a.iD)("div",m,[(0,a.Wm)(y,{class:"w-100",modelValue:e.row.destine_name,"onUpdate:modelValue":t=>e.row.destine_name=t,placeholder:"請選擇終點"},{default:(0,a.w5)((()=>[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(h.GetOption(e.row.action),(e=>((0,a.wg)(),(0,a.j4)(k,{key:e,label:e.name,value:e.tag},null,8,["label","value"])))),128))])),_:2},1032,["modelValue","onUpdate:modelValue"])]))])),_:1}),(0,a.Wm)(f,null,{default:(0,a.w5)((e=>[(0,a.Wm)(v,{onClick:t=>h.HandleDeleteHotRunAction(e.row),size:"small",type:"danger"},{default:(0,a.w5)((()=>[(0,a.Uk)("Delete")])),_:2},1032,["onClick"])])),_:1})])),_:1},8,["data"])])])),_:1},8,["modelValue"])]),(0,a.Wm)(g,{"content-position":"left"},{default:(0,a.w5)((()=>[(0,a.Uk)("進階設置")])),_:1}),(0,a._)("div",p,[(0,a.Wm)(x,{"label-position":"left"},{default:(0,a.w5)((()=>[(0,a.Wm)(U,{label:"*不接受隨機搬運任務AGV清單",class:"d-flex"},{default:(0,a.w5)((()=>[(0,a.Wm)(y,{class:"flex-fill",multiple:"",modelValue:_.noJoinRamdomCarryScriptAGVList,"onUpdate:modelValue":t[3]||(t[3]=e=>_.noJoinRamdomCarryScriptAGVList=e)},{default:(0,a.w5)((()=>[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(h.AgvNameList,(e=>((0,a.wg)(),(0,a.j4)(k,{key:e,label:e,value:e},null,8,["label","value"])))),128))])),_:1},8,["modelValue"]),(0,a.Wm)(v,{onClick:h.HandleSaveNoJoinRamdomCarryTaskAGVList,type:"primary"},{default:(0,a.w5)((()=>[(0,a.Uk)("儲存設定")])),_:1},8,["onClick"])])),_:1})])),_:1})])])}var _=l(24239),h=l(95320),g=l(8764),v=l(9669),f=l.n(v),b=l(663),k=l(81348),y={data(){return{hotRunScripts:[{no:1,scriptID:"",agv_name:"AGV_001",loop_num:10,finish_num:0,state:"IDLE",RealTimeMessage:"",IsRandomCarryRun:!1,actions:[{no:1,action:"move",source_tag:0,source_slot:0,destine_tag:2,destine_slot:0,cst_id:""},{no:2,action:"move",source_tag:0,destine_tag:2,cst_id:""}]}],action_drawer_visible:!1,selected_script_name:"123",selected_script_actions:[],noJoinRamdomCarryScriptAGVList:[]}},methods:{GetOption(e){return"move"==e?this.moveable_tags:"park"==e?this.parkable_tags:"load"==e||"unload"==e||"carry"==e?this.stock_tags:"charge"==e||"exchangebattery"==e?this.chargable_tags:"measure"==e?this.bay_tags:void 0},action_move_up(e){var t=this.selected_script_actions.indexOf(e);0!=t&&(this.selected_script_actions=this.move_element_up(this.selected_script_actions,t))},action_move_down(e){var t=this.selected_script_actions.indexOf(e);t!=this.selected_script_actions.length-1&&(this.selected_script_actions=this.move_element_down(this.selected_script_actions,t))},move_element_up(e,t){if(t>0&&t<e.length){const l=e[t];e.splice(t,1),e.splice(t-1,0,l);for(let t=0;t<e.length;t++)e[t].no=t+1;return e}},move_element_down(e,t){if(t>=0&&t<e.length-1){const l=e[t];e.splice(t,1),e.splice(t+1,0,l);for(let t=0;t<e.length;t++)e[t].no=t+1;return e}},slotSelectable(e){return"carry"==e||"unload"==e||"load"==e},async HandleStartBtnClick(e){"IDLE"==e.state?this.$swal.fire({text:"",title:"執行Hot Run ?",icon:"question",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((async t=>{if(t.isConfirmed){var l=await(0,g.Qk)(e.scriptID);l.confirm?this.$swal.fire({text:"",title:"HOT RUN Start!",icon:"success",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}):this.$swal.fire({text:l.message,title:"無法執行HOT RUN",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"})}})):this.$swal.fire({text:"",title:"確定要終止?",icon:"question",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((t=>{t.isConfirmed&&(0,g.AZ)(e.scriptID)}))},async HandleSaveBtnClick(){this.hotRunScripts.forEach((e=>{e.RealTimeMessage||(e.RealTimeMessage="")}));var e=await(0,g.A5)(this.hotRunScripts);this.$swal.fire({text:e.result?"":e.message,title:e.result?"儲存成功":"儲存失敗",icon:e.result?"success":"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert",timer:e.result?1e3:void 0})},async HandleSaveBtnClickInDrawer(){this.action_drawer_visible=!1,setTimeout((async()=>{await this.HandleSaveBtnClick(),setTimeout((()=>{this.action_drawer_visible=!0}),1e3)}),100)},HandleDeleteHotRunAction(e){var t=this.selected_script_actions.indexOf(e);this.selected_script_actions.splice(t,1)},HandleDeleteScript(e){this.$swal.fire({text:"",title:"確定要刪除此腳本?",icon:"warning",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((t=>{if(t.isConfirmed){"Running"==e.state&&(0,g.AZ)(e.no),this.hotRunScripts.splice(this.hotRunScripts.indexOf(e),1);for(let e=0;e<this.hotRunScripts.length;e++)this.hotRunScripts[e].no=e+1}}))},async HandleSaveNoJoinRamdomCarryTaskAGVList(){var e=f().create({baseURL:`${b.Z.vms_host}`}),t=await e.post("/api/task/SettingNoRunRandomCarryHotRunAGVList",this.noJoinRamdomCarryScriptAGVList);t.data&&k.z8.success({message:"設置成功"})}},computed:{AgvNameList(){return _.sn.getters.AGVNameList},moveable_tags(){return h.p.getters.AllNormalStationOptions},stock_tags(){return h.p.getters.AllEqStation},chargable_tags(){return h.p.getters.AllChargeStation},parkable_tags(){return h.p.getters.AllParkingStationOptions},bay_tags(){var e=h.p.getters.BaysData;return Object.keys(e).map((e=>({tag:e,name:e})))},hot_run_states(){return _.sn.getters.HotRunStates},SlotOptions(){return[{value:0,label:"第1層"},{value:1,label:"第2層"},{value:2,label:"第3層"}]}},mounted(){setTimeout((async()=>{this.hotRunScripts=await(0,g.gh)(),(0,a.YP)((()=>this.hot_run_states),((e,t)=>{for(let a=0;a<e.length;a++){const t=e[a];var l=this.hotRunScripts.find((e=>e.scriptID===t.scriptID));l&&(l.state=t.state,l.finish_num=t.finish_num,l.RealTimeMessage=t.RealTimeMessage)}}),{deep:!0,immediate:!0})}),100)}},C=l(83744);const V=(0,C.Z)(y,[["render",w],["__scopeId","data-v-420c910c"]]);var W=V}}]);
//# sourceMappingURL=326.487f5719.js.map