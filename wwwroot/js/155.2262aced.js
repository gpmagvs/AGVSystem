"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[155],{92155:function(e,t,a){a.r(t),a.d(t,{default:function(){return B}});a(57658);var l=a(66252),n=a(3577);const o={class:"p-2 text-start"},s={class:"w-100 border-bottom my-1"},i={class:"mx-2"},r=["id"],d={class:"px-2 text-start"},u={class:"text-start w-100"},m={key:0},c={key:1},p={class:"w-100 bg-light px-2 text-start"};function _(e,t,a,_,w,g){const h=(0,l.up)("el-divider"),R=(0,l.up)("el-button"),b=(0,l.up)("el-table-column"),v=(0,l.up)("el-checkbox"),S=(0,l.up)("el-option"),f=(0,l.up)("el-select"),U=(0,l.up)("el-tag"),k=(0,l.up)("el-input-number"),y=(0,l.up)("el-input"),V=(0,l.up)("el-table"),W=(0,l.up)("el-drawer"),C=(0,l.up)("ReqularUnloadHotRunSettings"),D=(0,l.up)("UpDownStreamSettings"),H=(0,l.up)("el-form-item"),I=(0,l.up)("el-form");return(0,l.wg)(),(0,l.iD)("div",null,[(0,l.Wm)(h,{"content-position":"left"},{default:(0,l.w5)((()=>[(0,l.Uk)("腳本")])),_:1}),(0,l._)("div",o,[(0,l._)("div",s,[(0,l.Wm)(R,{onClick:g.HandleSaveBtnClick,type:"primary"},{default:(0,l.w5)((()=>[(0,l.Uk)("儲存腳本設定")])),_:1},8,["onClick"]),(0,l.Wm)(R,{class:"my-1",type:"danger",onClick:t[0]||(t[0]=()=>{w.hotRunScripts.push({no:w.hotRunScripts.length+1,agv_name:void 0,loop_num:10,finish_num:0,action_num:9,state:"IDLE",actions:[]})})},{default:(0,l.w5)((()=>[(0,l.Uk)("新增動作")])),_:1})]),(0,l.Wm)(V,{"row-key":"scriptID",data:w.hotRunScripts,"default-expand-all":!1,border:""},{default:(0,l.w5)((()=>[(0,l.Wm)(b,{label:"NO.",prop:"no",width:"60",align:"center"}),(0,l.Wm)(b,{label:"ID",prop:"scriptID",width:"210",align:"center"}),(0,l.Wm)(b,{label:"隨機搬運",prop:"IsRandomCarryRun",align:"center"},{default:(0,l.w5)((e=>[(0,l.Wm)(v,{modelValue:e.row.IsRandomCarryRun,"onUpdate:modelValue":t=>e.row.IsRandomCarryRun=t},null,8,["modelValue","onUpdate:modelValue"])])),_:1}),(0,l.Wm)(b,{label:"定期出貨模擬",prop:"IsRegularUnloadRequst",align:"center"},{default:(0,l.w5)((e=>[(0,l.Wm)(v,{modelValue:e.row.IsRegularUnloadRequst,"onUpdate:modelValue":t=>e.row.IsRegularUnloadRequst=t},null,8,["modelValue","onUpdate:modelValue"])])),_:1}),(0,l.Wm)(b,{label:"執行AGV",prop:"agv_name",width:"150"},{default:(0,l.w5)((e=>[(0,l._)("div",null,[(0,l.Wm)(f,{disabled:e.row.IsRandomCarryRun,modelValue:e.row.agv_name,"onUpdate:modelValue":t=>e.row.agv_name=t},{default:(0,l.w5)((()=>[((0,l.wg)(!0),(0,l.iD)(l.HY,null,(0,l.Ko)(g.AgvNameList,(e=>((0,l.wg)(),(0,l.j4)(S,{key:e,label:e,value:e},null,8,["label","value"])))),128))])),_:2},1032,["disabled","modelValue","onUpdate:modelValue"])])])),_:1}),(0,l.Wm)(b,{label:"狀態",prop:"state",width:"120",align:"center"},{default:(0,l.w5)((e=>[(0,l._)("div",null,[(0,l.Wm)(U,{effect:"dark",type:"Running"==e.row.state?"success":"warning"},{default:(0,l.w5)((()=>[(0,l.Uk)((0,n.zw)(e.row.state),1)])),_:2},1032,["type"])])])),_:1}),(0,l.Wm)(b,{label:"已完成次數",prop:"finish_num",width:"100",align:"center"},{default:(0,l.w5)((e=>[(0,l._)("div",null,(0,n.zw)(e.row.finish_num)+"/"+(0,n.zw)(e.row.loop_num),1)])),_:1}),(0,l.Wm)(b,{label:"Loop次數",prop:"finish_num",width:"120",align:"center"},{default:(0,l.w5)((e=>[(0,l.Wm)(k,{disabled:e.row.IsRandomCarryRun,size:"small",style:{width:"80px"},step:1,precision:0,modelValue:e.row.loop_num,"onUpdate:modelValue":t=>e.row.loop_num=t},null,8,["disabled","modelValue","onUpdate:modelValue"])])),_:1}),(0,l.Wm)(b,{label:"動作數",width:"220"},{default:(0,l.w5)((e=>[(0,l._)("div",null,[(0,l._)("span",i,(0,n.zw)(e.row.actions.length),1),(0,l.Wm)(R,{disabled:e.row.IsRandomCarryRun,size:"small",onClick:()=>{e.row.IsRegularUnloadRequst?g.HandleRegularULDScriptActionBtnClick(e.row.RegularLoadSettings):w.action_drawer_visible=!0,w.selected_script_name=e.row.agv_name,w.selected_script_actions=e.row.actions}},{default:(0,l.w5)((()=>[(0,l.Uk)((0,n.zw)(e.row.IsRegularUnloadRequst?"設備出入料設定":"動作設定"),1)])),_:2},1032,["disabled","onClick"]),e.row.IsRandomCarryRun?((0,l.wg)(),(0,l.j4)(R,{key:0,type:"primary",size:"small",onClick:t=>g.HandleUpDownStreamSettingsBtnClick(e.row)},{default:(0,l.w5)((()=>[(0,l.Uk)("上下游設定")])),_:2},1032,["onClick"])):(0,l.kq)("",!0)])])),_:1}),(0,l.Wm)(b,{label:"Comment",width:"240"},{default:(0,l.w5)((e=>[(0,l.Wm)(y,{modelValue:e.row.comment,"onUpdate:modelValue":t=>e.row.comment=t},null,8,["modelValue","onUpdate:modelValue"])])),_:1}),(0,l.Wm)(b,{label:"即時資訊",prop:"RealTimeMessage",width:"auto"}),(0,l.Wm)(b,null,{default:(0,l.w5)((e=>[(0,l._)("div",null,[(0,l.Wm)(R,{type:"Running"==e.row.state?"danger":"success",size:"small",onClick:t=>g.HandleStartBtnClick(e.row)},{default:(0,l.w5)((()=>[(0,l.Uk)((0,n.zw)("Running"==e.row.state?"中止":"執行"),1)])),_:2},1032,["type","onClick"]),(0,l.Wm)(R,{size:"small",onClick:t=>g.HandleDeleteScript(e.row),type:"danger"},{default:(0,l.w5)((()=>[(0,l.Uk)("刪除")])),_:2},1032,["onClick"])])])),_:1})])),_:1},8,["data"]),(0,l.Wm)(W,{modelValue:w.action_drawer_visible,"onUpdate:modelValue":t[2]||(t[2]=e=>w.action_drawer_visible=e),direction:"rtl",size:"60%"},{header:(0,l.w5)((({titleId:e,titleClass:t})=>[(0,l._)("h4",{class:(0,n.C_)(["text-danger px-5 text-center",t]),id:e},"HOT RUN Actions Setting : "+(0,n.zw)(w.selected_script_name),11,r)])),default:(0,l.w5)((()=>[(0,l._)("div",d,[(0,l.Wm)(R,{class:"mx-2",type:"danger",onClick:t[1]||(t[1]=()=>{w.selected_script_actions.push({no:w.selected_script_actions.length+1,action:"move",source_tag:void 0,destine_tag:void 0})})},{default:(0,l.w5)((()=>[(0,l.Uk)("新增動作")])),_:1}),(0,l.Wm)(R,{class:"mx-2",onClick:g.HandleSaveBtnClickInDrawer,type:"primary"},{default:(0,l.w5)((()=>[(0,l.Uk)("儲存設定")])),_:1},8,["onClick"]),(0,l.Wm)(V,{"row-key":"no",style:{width:"1024px"},border:"",class:"m-2",data:w.selected_script_actions},{default:(0,l.w5)((()=>[(0,l.Wm)(b,{width:"50"},{default:(0,l.w5)((e=>[(0,l._)("div",u,[(0,l.Wm)(R,{class:"w-100",size:"small",onClick:t=>g.action_move_up(e.row)},{default:(0,l.w5)((()=>[(0,l.Uk)("▲")])),_:2},1032,["onClick"]),(0,l.Wm)(R,{class:"w-100",size:"small",onClick:t=>g.action_move_down(e.row)},{default:(0,l.w5)((()=>[(0,l.Uk)("▼")])),_:2},1032,["onClick"])])])),_:1}),(0,l.Wm)(b,{label:"NO.",prop:"no",width:"60",align:"center"}),(0,l.Wm)(b,{label:"動作",prop:"action",width:"120"},{default:(0,l.w5)((e=>[(0,l.Wm)(f,{class:"w-100",modelValue:e.row.action,"onUpdate:modelValue":t=>e.row.action=t,placeholder:"請選擇Action"},{default:(0,l.w5)((()=>[(0,l.Wm)(S,{label:"移動",value:"move"}),(0,l.Wm)(S,{label:"停車",value:"park"}),(0,l.Wm)(S,{label:"搬運",value:"carry"}),(0,l.Wm)(S,{label:"放貨",value:"load"}),(0,l.Wm)(S,{label:"取貨",value:"unload"}),(0,l.Wm)(S,{label:"充電",value:"charge"}),(0,l.Wm)(S,{label:"巡檢量測",value:"measure"}),(0,l.Wm)(S,{label:"交換電池",value:"exchangebattery"})])),_:2},1032,["modelValue","onUpdate:modelValue"])])),_:1}),(0,l.Wm)(b,{label:"起點",prop:"source_tag",width:"250"},{default:(0,l.w5)((e=>[(0,l.Wm)(f,{disabled:"carry"!=e.row.action,class:"w-100",modelValue:e.row.source_tag,"onUpdate:modelValue":t=>e.row.source_tag=t,placeholder:"請選擇起點"},{default:(0,l.w5)((()=>[((0,l.wg)(!0),(0,l.iD)(l.HY,null,(0,l.Ko)(g.GetOption(e.row.action),(e=>((0,l.wg)(),(0,l.j4)(S,{key:e,label:e.name,value:e.tag},null,8,["label","value"])))),128))])),_:2},1032,["disabled","modelValue","onUpdate:modelValue"]),g.slotSelectable(e.row.action)?((0,l.wg)(),(0,l.j4)(f,{key:0,disabled:"carry"!=e.row.action,class:"w-100",modelValue:e.row.source_slot,"onUpdate:modelValue":t=>e.row.source_slot=t,placeholder:"請選擇Slot"},{default:(0,l.w5)((()=>[((0,l.wg)(!0),(0,l.iD)(l.HY,null,(0,l.Ko)(g.SlotOptions,(e=>((0,l.wg)(),(0,l.j4)(S,{key:e.value,label:e.label,value:e.value},null,8,["label","value"])))),128))])),_:2},1032,["disabled","modelValue","onUpdate:modelValue"])):(0,l.kq)("",!0)])),_:1}),(0,l.Wm)(b,{label:"卡匣ID",width:"150"},{default:(0,l.w5)((e=>[(0,l.Wm)(y,{disabled:"carry"!=e.row.action&&"unload"!=e.row.action,modelValue:e.row.cst_id,"onUpdate:modelValue":t=>e.row.cst_id=t},null,8,["disabled","modelValue","onUpdate:modelValue"])])),_:1}),(0,l.Wm)(b,{label:"終點",prop:"destine_tag",width:"250"},{default:(0,l.w5)((e=>["measure"!=e.row.action?((0,l.wg)(),(0,l.iD)("div",m,[(0,l.Wm)(f,{class:"w-100",modelValue:e.row.destine_tag,"onUpdate:modelValue":t=>e.row.destine_tag=t,placeholder:"請選擇終點"},{default:(0,l.w5)((()=>[((0,l.wg)(!0),(0,l.iD)(l.HY,null,(0,l.Ko)(g.GetOption(e.row.action),(e=>((0,l.wg)(),(0,l.j4)(S,{key:e,label:e.name,value:e.tag},null,8,["label","value"])))),128))])),_:2},1032,["modelValue","onUpdate:modelValue"]),g.slotSelectable(e.row.action)?((0,l.wg)(),(0,l.j4)(f,{key:0,class:"w-100",modelValue:e.row.destine_slot,"onUpdate:modelValue":t=>e.row.destine_slot=t,placeholder:"請選擇Slot"},{default:(0,l.w5)((()=>[((0,l.wg)(!0),(0,l.iD)(l.HY,null,(0,l.Ko)(g.SlotOptions,(e=>((0,l.wg)(),(0,l.j4)(S,{key:e.value,label:e.label,value:e.value},null,8,["label","value"])))),128))])),_:2},1032,["modelValue","onUpdate:modelValue"])):(0,l.kq)("",!0)])):((0,l.wg)(),(0,l.iD)("div",c,[(0,l.Wm)(f,{class:"w-100",modelValue:e.row.destine_name,"onUpdate:modelValue":t=>e.row.destine_name=t,placeholder:"請選擇終點"},{default:(0,l.w5)((()=>[((0,l.wg)(!0),(0,l.iD)(l.HY,null,(0,l.Ko)(g.GetOption(e.row.action),(e=>((0,l.wg)(),(0,l.j4)(S,{key:e,label:e.name,value:e.tag},null,8,["label","value"])))),128))])),_:2},1032,["modelValue","onUpdate:modelValue"])]))])),_:1}),(0,l.Wm)(b,null,{default:(0,l.w5)((e=>[(0,l.Wm)(R,{onClick:t=>g.HandleDeleteHotRunAction(e.row),size:"small",type:"danger"},{default:(0,l.w5)((()=>[(0,l.Uk)("Delete")])),_:2},1032,["onClick"])])),_:1})])),_:1},8,["data"])])])),_:1},8,["modelValue"]),(0,l.Wm)(W,{modelValue:w.regular_unload_settings_drawer_visible,"onUpdate:modelValue":t[3]||(t[3]=e=>w.regular_unload_settings_drawer_visible=e),direction:"rtl",size:"60%"},{default:(0,l.w5)((()=>[(0,l.Wm)(C,{ref:"RegularSettings",regular_unload_settings:w.selected_regular_unload_settings},null,8,["regular_unload_settings"])])),_:1},8,["modelValue"]),(0,l.Wm)(W,{modelValue:w.updown_stream_settings_drawer_visible,"onUpdate:modelValue":t[4]||(t[4]=e=>w.updown_stream_settings_drawer_visible=e),title:"上下游設定",direction:"rtl",size:"60%"},{default:(0,l.w5)((()=>[(0,l.Wm)(D,{ref:"UpDownStreamSettings"},null,512)])),_:1},8,["modelValue"])]),(0,l.Wm)(h,{"content-position":"left"},{default:(0,l.w5)((()=>[(0,l.Uk)("進階設置")])),_:1}),(0,l._)("div",p,[(0,l.Wm)(I,{"label-position":"left"},{default:(0,l.w5)((()=>[(0,l.Wm)(H,{label:"*不接受隨機搬運任務AGV清單",class:"d-flex"},{default:(0,l.w5)((()=>[(0,l.Wm)(f,{class:"flex-fill",multiple:"",modelValue:w.noJoinRamdomCarryScriptAGVList,"onUpdate:modelValue":t[5]||(t[5]=e=>w.noJoinRamdomCarryScriptAGVList=e)},{default:(0,l.w5)((()=>[((0,l.wg)(!0),(0,l.iD)(l.HY,null,(0,l.Ko)(g.AgvNameList,(e=>((0,l.wg)(),(0,l.j4)(S,{key:e,label:e,value:e},null,8,["label","value"])))),128))])),_:1},8,["modelValue"]),(0,l.Wm)(R,{onClick:g.HandleSaveNoJoinRamdomCarryTaskAGVList,type:"primary"},{default:(0,l.w5)((()=>[(0,l.Uk)("儲存設定")])),_:1},8,["onClick"])])),_:1})])),_:1})])])}var w=a(25044),g=a(95320),h=a(8764);function R(e,t,a,n,o,s){const i=(0,l.up)("el-option"),r=(0,l.up)("el-select"),d=(0,l.up)("el-form-item"),u=(0,l.up)("el-form"),m=(0,l.up)("el-table-column"),c=(0,l.up)("el-input"),p=(0,l.up)("el-table");return(0,l.wg)(),(0,l.iD)("div",null,[(0,l.Wm)(u,{"label-position":"left","label-width":"120"},{default:(0,l.w5)((()=>[(0,l.Wm)(d,{label:"可入料的設備"},{default:(0,l.w5)((()=>[(0,l.Wm)(r,{multiple:"",modelValue:o._regular_unload_settings.LoadRequestAlwaysOnEqNames,"onUpdate:modelValue":t[0]||(t[0]=e=>o._regular_unload_settings.LoadRequestAlwaysOnEqNames=e)},{default:(0,l.w5)((()=>[((0,l.wg)(!0),(0,l.iD)(l.HY,null,(0,l.Ko)(s.EQPortOptions,(e=>((0,l.wg)(),(0,l.j4)(i,{key:e.value,label:e.name,value:e.name},null,8,["label","value"])))),128))])),_:1},8,["modelValue"])])),_:1}),(0,l.Wm)(d)])),_:1}),(0,l.Wm)(p,{data:o.tableModel,height:"900",border:""},{default:(0,l.w5)((()=>[(0,l.Wm)(m,{label:"EQ名稱",prop:"name"}),(0,l.Wm)(m,{label:"Tag",prop:"tag"}),(0,l.Wm)(m,{label:"收/入料"},{default:(0,l.w5)((e=>[(0,l.Wm)(r,{modelValue:e.row.lduld,"onUpdate:modelValue":t=>e.row.lduld=t,onChange:t=>s.HandleUdUldChanged(e.row)},{default:(0,l.w5)((()=>[(0,l.Wm)(i,{label:"出料",value:"unload"}),(0,l.Wm)(i,{label:"入料",value:"load"}),(0,l.Wm)(i,{label:"IDLE/BUSY",value:"busy"})])),_:2},1032,["modelValue","onUpdate:modelValue","onChange"])])),_:1}),(0,l.Wm)(m,{label:"延遲出料請求(秒)(腳本啟動後)"},{default:(0,l.w5)((e=>[(0,l.Wm)(c,{disabled:"unload"!=e.row.lduld,type:"number",modelValue:e.row.DelayTimeWhenScriptStart,"onUpdate:modelValue":t=>e.row.DelayTimeWhenScriptStart=t,onChange:t=>s.HandleDelayTimeOnScriptStartChanged(e.row)},null,8,["disabled","modelValue","onUpdate:modelValue","onChange"])])),_:1}),(0,l.Wm)(m,{label:"出料節拍(秒)"},{default:(0,l.w5)((e=>[(0,l.Wm)(c,{disabled:"unload"!=e.row.lduld,type:"number",modelValue:e.row.UnloadRequestInterval,"onUpdate:modelValue":t=>e.row.UnloadRequestInterval=t,onChange:t=>s.HandleUnloadIntervalChanged(e.row)},null,8,["disabled","modelValue","onUpdate:modelValue","onChange"])])),_:1})])),_:1},8,["data"])])}class b{constructor(){this.LoadRequestAlwaysOnEqNames=[],this.UnloadRequestsSettings=[new v]}}class v{constructor(e="",t=10,a=10){this.EqName=e,this.UnloadRequestInterval=t,this.DelayTimeWhenScriptStart=a}}var S={props:{regular_unload_settings:{type:Object,default(){return new b}}},data(){return{_regular_unload_settings:new b,tableModel:[]}},methods:{HandleUdUldChanged(e={name:opt.Name,tag:opt.TagID,lduld:"busy",DelayTimeWhenScriptStart:1,UnloadRequestInterval:10}){var t=this._regular_unload_settings.LoadRequestAlwaysOnEqNames.indexOf(e.name);if(-1!=t&&this._regular_unload_settings.LoadRequestAlwaysOnEqNames.splice(t,1),t=this._regular_unload_settings.UnloadRequestsSettings.findIndex((t=>t.EqName==e.name)),-1!=t&&this._regular_unload_settings.UnloadRequestsSettings.splice(t,1),"load"==e.lduld&&this._regular_unload_settings.LoadRequestAlwaysOnEqNames.push(e.name),"unload"==e.lduld){var a=new v(e.name,e.UnloadRequestInterval,e.DelayTimeWhenScriptStart);this._regular_unload_settings.UnloadRequestsSettings.push(a)}},HandleDelayTimeOnScriptStartChanged(e={name:opt.Name,tag:opt.TagID,lduld:"busy",DelayTimeWhenScriptStart:1,UnloadRequestInterval:10}){var t=this._regular_unload_settings.UnloadRequestsSettings.find((t=>t.EqName==e.name));t&&(t.DelayTimeWhenScriptStart=e.DelayTimeWhenScriptStart)},HandleUnloadIntervalChanged(e={name:opt.Name,tag:opt.TagID,lduld:"busy",DelayTimeWhenScriptStart:1,UnloadRequestInterval:10}){var t=this._regular_unload_settings.UnloadRequestsSettings.find((t=>t.EqName==e.name));t&&(t.UnloadRequestInterval=e.UnloadRequestInterval)},update(e){this._regular_unload_settings=e;var t=w.jU.state.EqOptions.map((e=>({name:e.Name,tag:e.TagID,lduld:"busy",DelayTimeWhenScriptStart:1,UnloadRequestInterval:10})));this._regular_unload_settings.LoadRequestAlwaysOnEqNames.forEach((e=>{var a=t.find((t=>t.name==e));a&&(a.lduld="load")})),this._regular_unload_settings.UnloadRequestsSettings.forEach((e=>{var a=t.find((t=>t.name==e.EqName));a&&(a.lduld="unload",a.UnloadRequestInterval=e.UnloadRequestInterval,a.DelayTimeWhenScriptStart=e.DelayTimeWhenScriptStart)})),this.tableModel=t}},computed:{EQPortOptions(){return w.jU.state.EqOptions.map((e=>({name:e.Name,value:e.TagID})))}}},f=a(83744);const U=(0,f.Z)(S,[["render",R]]);var k=U;const y=e=>((0,l.dD)("data-v-b8fbb822"),e=e(),(0,l.Cn)(),e),V={class:"updown-stream-settings p-3"},W=y((()=>(0,l._)("h5",null,"Options",-1))),C={class:"d-flex flex-column w-100 mb-5"},D=y((()=>(0,l._)("h5",null,"Rack Up/Down Stream",-1)));function H(e,t,a,o,s,i){const r=(0,l.up)("el-checkbox"),d=(0,l.up)("el-table-column"),u=(0,l.up)("el-option"),m=(0,l.up)("el-select"),c=(0,l.up)("el-table");return(0,l.wg)(),(0,l.iD)("div",V,[(0,l._)("h3",null,(0,n.zw)(i.scriptName),1),W,(0,l._)("div",C,[(0,l.Wm)(r,{size:"large",label:"Rack Port需要真正有貨才可做為起點",modelValue:s.script.RandomHotRunSettings.IsRackPortNeedHasCargoAcutally,"onUpdate:modelValue":t[0]||(t[0]=e=>s.script.RandomHotRunSettings.IsRackPortNeedHasCargoAcutally=e)},null,8,["modelValue"]),(0,l.Wm)(r,{size:"large",label:"主設備出貨後僅搬運至Rack",modelValue:s.script.RandomHotRunSettings.IsMainEqUnloadTransferToRackOnly,"onUpdate:modelValue":t[1]||(t[1]=e=>s.script.RandomHotRunSettings.IsMainEqUnloadTransferToRackOnly=e)},null,8,["modelValue"]),(0,l.Wm)(r,{size:"large",label:"僅取放Rack第一層",modelValue:s.script.RandomHotRunSettings.IsOnlyUseRackFirstLayer,"onUpdate:modelValue":t[2]||(t[2]=e=>s.script.RandomHotRunSettings.IsOnlyUseRackFirstLayer=e)},null,8,["modelValue"])]),D,(0,l.Wm)(c,{data:s.RackTable,border:""},{default:(0,l.w5)((()=>[(0,l.Wm)(d,{label:"RACK",prop:"Name"}),(0,l.Wm)(d,{label:"可供入料取貨之設備"},{default:(0,l.w5)((e=>[(0,l.Wm)(m,{modelValue:e.row.DownStream,"onUpdate:modelValue":t=>e.row.DownStream=t,multiple:"",onChange:t=>i.HandleDownStreamChanged(e.row)},{default:(0,l.w5)((()=>[((0,l.wg)(!0),(0,l.iD)(l.HY,null,(0,l.Ko)(i.EQPortOptions,(e=>((0,l.wg)(),(0,l.j4)(u,{key:e.value,label:e.name,value:e.value},null,8,["label","value"])))),128))])),_:2},1032,["modelValue","onUpdate:modelValue","onChange"])])),_:1}),(0,l.Wm)(d,{label:"可供出料存貨之設備"},{default:(0,l.w5)((e=>[(0,l.Wm)(m,{modelValue:e.row.UpStream,"onUpdate:modelValue":t=>e.row.UpStream=t,multiple:"",onChange:t=>i.HandleUpStreamChanged(e.row)},{default:(0,l.w5)((()=>[((0,l.wg)(!0),(0,l.iD)(l.HY,null,(0,l.Ko)(i.EQPortOptions,(e=>((0,l.wg)(),(0,l.j4)(u,{key:e.value,label:e.name,value:e.value},null,8,["label","value"])))),128))])),_:2},1032,["modelValue","onUpdate:modelValue","onChange"])])),_:1})])),_:1},8,["data"]),(0,l.kq)("",!0),(0,l.kq)("",!0)])}var I={name:"UpDownStreamSettings",data(){return{script:{RandomHotRunSettings:{}},RackTable:[{Name:"Rack1"}]}},computed:{scriptName(){return this.script?this.script.scriptID??"Unknown Script":"Unknown Script"},EQPortOptions(){return w.jU.state.EqOptions.map((e=>({name:e.Name+`(${e.TagID})`,value:e.TagID})))},RacksInfo(){return w.jU.state.WIPOptions.map((e=>({name:e.Name,value:e.TagID})))}},methods:{update(e){this.script=e,this.RackTable=this.RacksInfo.map((e=>({Name:e.name,UpStream:[],DownStream:[]}))),e.RandomHotRunSettings&&e.RandomHotRunSettings.RacksUpDownStarems&&Object.entries(e.RandomHotRunSettings.RacksUpDownStarems).forEach((([e,t])=>{const a=this.RackTable.find((t=>t.Name==e));a&&(a.UpStream=t.UpStream,a.DownStream=t.DownStream)}))},HandleDownStreamChanged(e){console.log(e),this.script.RandomHotRunSettings.RacksUpDownStarems[e.Name]||(this.script.RandomHotRunSettings.RacksUpDownStarems[e.Name]={UpStream:[],DownStream:[]}),this.script.RandomHotRunSettings.RacksUpDownStarems[e.Name].DownStream=e.DownStream},HandleUpStreamChanged(e){this.script.RandomHotRunSettings.RacksUpDownStarems[e.Name]||(this.script.RandomHotRunSettings.RacksUpDownStarems[e.Name]={UpStream:[],DownStream:[]}),this.script.RandomHotRunSettings.RacksUpDownStarems[e.Name].UpStream=e.UpStream}}};const q=(0,f.Z)(I,[["render",H],["__scopeId","data-v-b8fbb822"]]);var T=q,O=a(9669),N=a.n(O),x=a(663),A=a(81348),E={components:{ReqularUnloadHotRunSettings:k,UpDownStreamSettings:T},data(){return{hotRunScripts:[{no:1,scriptID:"",agv_name:"AGV_001",loop_num:10,finish_num:0,state:"IDLE",RealTimeMessage:"",IsRandomCarryRun:!1,actions:[{no:1,action:"move",source_tag:0,source_slot:0,destine_tag:2,destine_slot:0,cst_id:""},{no:2,action:"move",source_tag:0,destine_tag:2,cst_id:""}]}],action_drawer_visible:!1,regular_unload_settings_drawer_visible:!1,updown_stream_settings_drawer_visible:!1,selected_script_name:"123",selected_script_actions:[],selected_regular_unload_settings:{},noJoinRamdomCarryScriptAGVList:[]}},methods:{GetOption(e){return"move"==e?this.moveable_tags:"park"==e?this.parkable_tags:"load"==e||"unload"==e||"carry"==e?this.stock_tags:"charge"==e||"exchangebattery"==e?this.chargable_tags:"measure"==e?this.bay_tags:void 0},action_move_up(e){var t=this.selected_script_actions.indexOf(e);0!=t&&(this.selected_script_actions=this.move_element_up(this.selected_script_actions,t))},action_move_down(e){var t=this.selected_script_actions.indexOf(e);t!=this.selected_script_actions.length-1&&(this.selected_script_actions=this.move_element_down(this.selected_script_actions,t))},move_element_up(e,t){if(t>0&&t<e.length){const a=e[t];e.splice(t,1),e.splice(t-1,0,a);for(let t=0;t<e.length;t++)e[t].no=t+1;return e}},move_element_down(e,t){if(t>=0&&t<e.length-1){const a=e[t];e.splice(t,1),e.splice(t+1,0,a);for(let t=0;t<e.length;t++)e[t].no=t+1;return e}},slotSelectable(e){return"carry"==e||"unload"==e||"load"==e},async HandleStartBtnClick(e){"IDLE"==e.state?this.$swal.fire({text:"",title:"執行Hot Run ?",icon:"question",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((async t=>{if(t.isConfirmed){var a=await(0,h.Qk)(e.scriptID);a.confirm?this.$swal.fire({text:"",title:"HOT RUN Start!",icon:"success",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}):this.$swal.fire({text:a.message,title:"無法執行HOT RUN",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"})}})):this.$swal.fire({text:"",title:"確定要終止?",icon:"question",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((t=>{t.isConfirmed&&(0,h.AZ)(e.scriptID)}))},async HandleSaveBtnClick(){this.hotRunScripts.forEach((e=>{e.RealTimeMessage||(e.RealTimeMessage="")}));var e=await(0,h.A5)(this.hotRunScripts);this.$swal.fire({text:e.result?"":e.message,title:e.result?"儲存成功":"儲存失敗",icon:e.result?"success":"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert",timer:e.result?1e3:void 0}),e.result&&setTimeout((()=>{location.reload()}),1e3)},async HandleSaveBtnClickInDrawer(){this.action_drawer_visible=!1,setTimeout((async()=>{await this.HandleSaveBtnClick(),setTimeout((()=>{this.action_drawer_visible=!0}),1e3)}),100)},HandleDeleteHotRunAction(e){var t=this.selected_script_actions.indexOf(e);this.selected_script_actions.splice(t,1)},HandleDeleteScript(e){this.$swal.fire({text:"",title:"確定要刪除此腳本?",icon:"warning",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((t=>{if(t.isConfirmed){"Running"==e.state&&(0,h.AZ)(e.no),this.hotRunScripts.splice(this.hotRunScripts.indexOf(e),1);for(let e=0;e<this.hotRunScripts.length;e++)this.hotRunScripts[e].no=e+1}}))},HandleRegularULDScriptActionBtnClick(e){this.selected_regular_unload_settings=e,this.regular_unload_settings_drawer_visible=!0,setTimeout((()=>{this.$refs["RegularSettings"].update(this.selected_regular_unload_settings)}),100)},HandleUpDownStreamSettingsBtnClick(e){this.updown_stream_settings_drawer_visible=!0,setTimeout((()=>{this.$refs["UpDownStreamSettings"].update(e)}),100)},async HandleSaveNoJoinRamdomCarryTaskAGVList(){var e=N().create({baseURL:`${x.Z.vms_host}`}),t=await e.post("/api/task/SettingNoRunRandomCarryHotRunAGVList",this.noJoinRamdomCarryScriptAGVList);t.data&&A.z8.success({message:"設置成功"})}},computed:{AgvNameList(){return w.sn.getters.AGVNameList},moveable_tags(){return g.p.getters.AllNormalStationOptions},stock_tags(){return g.p.getters.AllEqStation},chargable_tags(){return g.p.getters.AllChargeStation},parkable_tags(){return g.p.getters.AllParkingStationOptions},bay_tags(){var e=g.p.getters.BaysData;return Object.keys(e).map((e=>({tag:e,name:e})))},hot_run_states(){return w.sn.getters.HotRunStates},SlotOptions(){return[{value:0,label:"第1層"},{value:1,label:"第2層"},{value:2,label:"第3層"}]}},mounted(){setTimeout((async()=>{this.hotRunScripts=await(0,h.gh)(),(0,l.YP)((()=>this.hot_run_states),((e,t)=>{if(e)for(let l=0;l<e.length;l++){const t=e[l];var a=this.hotRunScripts.find((e=>e.scriptID===t.scriptID));a&&(a.state=t.state,a.finish_num=t.finish_num,a.RealTimeMessage=t.RealTimeMessage)}}),{deep:!0,immediate:!0})}),100)}};const L=(0,f.Z)(E,[["render",_],["__scopeId","data-v-1f688726"]]);var B=L}}]);
//# sourceMappingURL=155.2262aced.js.map