"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[740],{99740:function(e,t,a){a.r(t),a.d(t,{default:function(){return X}});var l=a(79003);const o={class:"d-flex"},s={class:"actions w-50"};function n(e,t,a,n,d,i){const r=(0,l.resolveComponent)("AGVStatus"),c=(0,l.resolveComponent)("Dispatcher"),m=(0,l.resolveComponent)("task-status-vue"),u=(0,l.resolveComponent)("TaskAllocationVue");return(0,l.openBlock)(),(0,l.createElementBlock)("div",null,[(0,l.createElementVNode)("div",o,[(0,l.createElementVNode)("div",s,[(0,l.createVNode)(r),(0,l.createVNode)(c)]),(0,l.createVNode)(m,{class:"flex-fill",show_card_title:!1,display_mode:"column"})]),(0,l.createVNode)(u)])}var d=a(55789),i=a(64371);const r=e=>((0,l.pushScopeId)("data-v-b99541ea"),e=e(),(0,l.popScopeId)(),e),c={class:"dispatcher border rounded p-2"},m={class:"d-flex p-2 border"},u={class:"border mx-2"},h={class:"bg-primary text-light w-100"},p=r((()=>(0,l.createElementVNode)("div",{class:"p-2 border"},null,-1)));function N(e,t,a,o,s,n){const d=(0,l.resolveComponent)("b-button"),i=(0,l.resolveComponent)("el-table-column"),r=(0,l.resolveComponent)("el-button"),N=(0,l.resolveComponent)("el-table"),V=(0,l.resolveComponent)("b-tab"),b=(0,l.resolveComponent)("b-tabs"),y=(0,l.resolveComponent)("NewScheduleHelper");return(0,l.openBlock)(),(0,l.createElementBlock)("div",c,[(0,l.createVNode)(b,null,{default:(0,l.withCtx)((()=>[(0,l.createVNode)(V,{title:"排程派車"},{default:(0,l.withCtx)((()=>[(0,l.createElementVNode)("div",m,[(0,l.createElementVNode)("div",null,[(0,l.createVNode)(d,{onClick:t[0]||(t[0]=()=>{e.$refs.schedule_add_helper.Show()}),variant:"primary"},{default:(0,l.withCtx)((()=>[(0,l.createTextVNode)("新增排程")])),_:1})]),(0,l.createElementVNode)("div",u,[(0,l.createElementVNode)("label",h,"排程列表("+(0,l.toDisplayString)(s.SecheulData.length)+")",1),(0,l.createVNode)(N,{style:{height:"350px"},data:s.SecheulData,"highlight-current-row":"",onRowClick:t[1]||(t[1]=(e,t,a)=>{s.selectedScheduleData=e})},{default:(0,l.withCtx)((()=>[(0,l.createVNode)(i,{label:"時間","min-width":"160",prop:"Time"}),(0,l.createVNode)(i,{label:"車輛","min-width":"160",prop:"AGVName"}),(0,l.createVNode)(i,{label:"Bay數量","min-width":"160",prop:"Bays"},{default:(0,l.withCtx)((e=>[(0,l.createTextVNode)((0,l.toDisplayString)(e.row.Bays.length),1)])),_:1}),(0,l.createVNode)(i,{label:"操作","min-width":"150"},{default:(0,l.withCtx)((e=>[(0,l.createVNode)(r,{size:"small",type:"primary",onClick:n.EditBtnClickHandler},{default:(0,l.withCtx)((()=>[(0,l.createTextVNode)("檢視")])),_:1},8,["onClick"]),(0,l.createVNode)(r,{size:"small",type:"danger",onClick:t=>n.HandleDeleteScheduleClick(e.row)},{default:(0,l.withCtx)((()=>[(0,l.createTextVNode)("刪除")])),_:2},1032,["onClick"])])),_:1})])),_:1},8,["data"])])])])),_:1}),(0,l.createVNode)(V,{title:"即時派車"},{default:(0,l.withCtx)((()=>[p])),_:1})])),_:1}),(0,l.createVNode)(y,{onOn_submit:n.HandleSchedulesChanged,ref:"schedule_add_helper"},null,8,["onOn_submit"]),(0,l.createVNode)(y,{onOn_submit:n.HandleSchedulesChanged,edit_mode:!0,ref:"schedule_edit_helper"},null,8,["onOn_submit"])])}const V=e=>((0,l.pushScopeId)("data-v-b05f48d2"),e=e(),(0,l.popScopeId)(),e),b={class:"login-header"},y=["id"],C={class:"copntent"},S={class:"d-flex text-start p-1"},g={class:"options border p-2",style:{width:"700px"}},w={class:"border rounded p-2 my-2"},f=V((()=>(0,l.createElementVNode)("div",{class:"label"},[(0,l.createElementVNode)("i",{class:"bi bi-clock"}),(0,l.createTextVNode)("時間設定")],-1))),v={class:"border rounded p-2 my-2"},_=V((()=>(0,l.createElementVNode)("div",{class:"label"},[(0,l.createElementVNode)("i",{class:"bi bi-truck-front"}),(0,l.createTextVNode)("指派車輛")],-1))),B={class:"border rounded p-2 my-2"},k=V((()=>(0,l.createElementVNode)("div",{class:"label"},[(0,l.createElementVNode)("i",{class:"bi bi-geo"}),(0,l.createTextVNode)("量測Bay選取")],-1))),x=V((()=>(0,l.createElementVNode)("div",{class:"d-flex flex-column"},null,-1))),T={class:"",style:{height:"350px"}};function D(e,t,a,o,s,n){const d=(0,l.resolveComponent)("el-divider"),i=(0,l.resolveComponent)("el-time-picker"),r=(0,l.resolveComponent)("el-option"),c=(0,l.resolveComponent)("el-select"),m=(0,l.resolveComponent)("el-table-column"),u=(0,l.resolveComponent)("el-table"),h=(0,l.resolveComponent)("b-button"),p=(0,l.resolveComponent)("Map"),N=(0,l.resolveComponent)("el-dialog");return(0,l.openBlock)(),(0,l.createBlock)(N,{modelValue:s.ShowDialog,"onUpdate:modelValue":t[2]||(t[2]=e=>s.ShowDialog=e),"show-close":!0,"close-on-click-modal":!0,"close-on-press-escape":!0,modal:!1,draggable:"",width:"800",fullscreen:"",style:{"z-index":"29900"}},{header:(0,l.withCtx)((({titleId:e,login_title:t})=>[(0,l.createElementVNode)("div",b,[(0,l.createElementVNode)("h3",{id:e,class:(0,l.normalizeClass)(t)},(0,l.toDisplayString)(s.Title),11,y),(0,l.createVNode)(d)])])),default:(0,l.withCtx)((()=>[(0,l.createElementVNode)("div",C,[(0,l.createElementVNode)("div",S,[(0,l.createElementVNode)("div",g,[(0,l.createElementVNode)("div",w,[f,(0,l.createVNode)(i,{format:"HH:mm",modelValue:s.schedule_settigs.Time,"onUpdate:modelValue":t[0]||(t[0]=e=>s.schedule_settigs.Time=e),type:"time",placeholder:"選擇量測時間"},null,8,["modelValue"])]),(0,l.createElementVNode)("div",v,[_,(0,l.createVNode)(c,{modelValue:s.schedule_settigs.AGVName,"onUpdate:modelValue":t[1]||(t[1]=e=>s.schedule_settigs.AGVName=e),placeholder:"選擇車輛",style:{width:"220px"}},{default:(0,l.withCtx)((()=>[((0,l.openBlock)(!0),(0,l.createElementBlock)(l.Fragment,null,(0,l.renderList)(n.AgvNameList,(e=>((0,l.openBlock)(),(0,l.createBlock)(r,{key:e,label:e,value:e},null,8,["label","value"])))),128))])),_:1},8,["modelValue"])]),(0,l.createElementVNode)("div",B,[k,x,(0,l.createElementVNode)("div",null,[(0,l.createElementVNode)("div",T,[(0,l.createVNode)(u,{border:"",data:s.BayTableData,ref:"table_ref",onSelectionChange:n.handleSelectionChange},{default:(0,l.withCtx)((()=>[(0,l.createVNode)(m,{type:"selection",label:"選取"}),(0,l.createVNode)(m,{prop:"BayName",label:"Bay名稱",width:"100"}),(0,l.createVNode)(m,{prop:"BayName",label:"量測點",width:"400"},{default:(0,l.withCtx)((e=>[(0,l.createVNode)(c,{onClick:t=>n.HandlePointsColumnClick(e.row),modelValue:e.row.SelectedPointNames,"onUpdate:modelValue":t=>e.row.SelectedPointNames=t,multiple:"",style:{width:"100%"},onChange:n.HandlePointsSelectedChange},{default:(0,l.withCtx)((()=>[((0,l.openBlock)(!0),(0,l.createElementBlock)(l.Fragment,null,(0,l.renderList)(e.row.PointNames,(e=>((0,l.openBlock)(),(0,l.createBlock)(r,{key:e,label:e,value:e},null,8,["label","value"])))),128))])),_:2},1032,["onClick","modelValue","onUpdate:modelValue","onChange"])])),_:1}),(0,l.createVNode)(m,{label:"量測順序"},{default:(0,l.withCtx)((e=>[(0,l.createVNode)(c,{modelValue:e.row.Sequence,"onUpdate:modelValue":t=>e.row.Sequence=t,onClick:t=>n.HandlePointsColumnClick(e.row),onChange:n.HandleSequenceChange},{default:(0,l.withCtx)((()=>[((0,l.openBlock)(!0),(0,l.createElementBlock)(l.Fragment,null,(0,l.renderList)(s.SequenceList,(e=>((0,l.openBlock)(),(0,l.createBlock)(r,{key:e,label:e,value:e},null,8,["label","value"])))),128))])),_:2},1032,["modelValue","onUpdate:modelValue","onClick","onChange"])])),_:1})])),_:1},8,["data","onSelectionChange"])])])]),(0,l.createVNode)(h,{disabled:n.IsNoBaySelected||""==s.schedule_settigs.AGVName||""==s.schedule_settigs.Time,onClick:n.HandleAddNewScheduleClick,style:{cursor:"pointer"},class:"w-100 my-3",variant:"primary"},{default:(0,l.withCtx)((()=>[(0,l.createTextVNode)((0,l.toDisplayString)(a.edit_mode?"修改":"新增排程"),1)])),_:1},8,["disabled","onClick"])]),(0,l.createVNode)(p,{class:"bg-light border rounded px-1 mx-1",id:"schedule_map",agv_show:!1,style:{"padding-right":"10px",width:"900px"}})])])])),_:1},8,["modelValue"])}a(57658);var E=a(27056),A=a(95320),G=a(24239),H=a(36797),q=a.n(H),P=a(56265),$=a.n(P),I=a(663),O=$().create({baseURL:I.Z.backend_host});async function L(){var e=await O.get("/api/InstrumentMeasure/GetBaysTableData");return e.data}var F=a(3567),M={components:{Map:E["default"]},props:{edit_mode:{type:Boolean,default:!1}},data(){return{ShowDialog:!1,Title:"新增量測排程",region_mode:"局部選擇",selected_row:{},previous_seq:0,modified:{Time:"",AGVName:""},schedule_settigs:{Time:"1991/12/20 12:10:20",Bays:[],AGVName:"AGV_001",ScriptName:"script_name"},BayTableData:[{BayName:"Bay1",PointNames:["AAA12","2","3"],SelectedPointNames:["AAA12","3"],Sequence:1},{BayName:"Bay2",PointNames:["1","2","3"],SelectedPointNames:["1","3"],Sequence:2}],SequenceList:[]}},computed:{map_station_data(){return A.p.getters.MapStations},BaysData(){return A.p.getters.BaysData},BayNames(){return Object.keys(A.p.getters.BaysData)},IsAllSeclted(){return this.BayNames.length==this.schedule_settigs.Bays.length|0==this.schedule_settigs.Bays.length},AgvNameList(){return G.sn.getters.AGVNameList},IsNoBaySelected(){return this.BayTableData.every((e=>0==e.Selected))}},methods:{async Show(){this.ShowDialog=!0,this.CreateDafualt(!0)},EditorMode(e){this.CreateDafualt(!1);var t=q()(e.Time).format("HH:mm");this.Title=`排程量測 - ${t}`,this.modified.Time=t,this.modified.AGVName=e.AGVName,this.schedule_settigs=e,setTimeout((()=>{var t=e.Bays.map((e=>e.BayName));this.BayTableData.forEach((a=>{var l=t.includes(a.BayName);if(a.Selected=l,l){var o=e.Bays.find((e=>e.BayName==a.BayName));a.SelectedPointNames=o.SelectedPointNames,a.Sequence=o.Sequence}this.$refs["table_ref"].toggleRowSelection(a,l)}))}),420),this.ShowDialog=!0},CreateDafualt(e){L().then((t=>{this.BayTableData=t,this.SequenceList=[];var a=this.BayTableData.length;for(let e=0;e<a;e++)this.SequenceList.push(e+1);setTimeout((()=>{this.BayTableData.forEach((t=>{t&&(t.Selected=e,this.$refs["table_ref"].toggleRowSelection(t,e))}))}),200)}))},HandlePointsColumnClick(e){this.selected_row=e,this.previous_seq=e.Sequence},async HandleAddNewScheduleClick(){this.schedule_settigs.Bays=this.BayTableData.filter((e=>e.Selected));var e=JSON.parse(JSON.stringify(this.schedule_settigs));e.Time=q()(e.Time).format("HH:mm"),this.ShowDialog=!1,this.$swal.fire({html:`<b>時間:</b>${e.Time} <br/><b>AGV:</b>${e.AGVName} <br/><b>量測Bay數量:</b>${e.Bays.length}`,title:this.edit_mode?"確定要修改此排程":"確定要新增此排程?",icon:"question",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((async t=>{if(t.isConfirmed){var a={result:!1,message:""};a=this.edit_mode?await(0,F.x0)(this.modified.Time,this.modified.AGVName,e):await(0,F.Wp)(e),this.$swal.fire({text:"",title:this.edit_mode?a.result?"修改成功":"修改失敗":a.result?"新增成功":"新增失敗",icon:a.result?"success":"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((()=>{this.ShowDialog=!0})),this.$emit("on_submit","")}else this.ShowDialog=!0}))},HandlePointsSelectedChange(e){this.selected_row.SelectedPointNames=e},HandleSequenceChange(e){var t=this.BayTableData.find((t=>t.Sequence==e));t.Sequence=this.previous_seq,this.selected_row.Sequence=e},handleSelectionChange(e){var t=e.map((e=>e.BayName));this.BayTableData.forEach((e=>{e.Selected=t.includes(e.BayName)}))}},mounted(){}},U=a(40089);const Z=(0,U.Z)(M,[["render",D],["__scopeId","data-v-b05f48d2"]]);var R=Z,z={components:{NewScheduleHelper:R},data(){return{SecheulData:[{Time:"1991/12/20 12:10:20",Bays:[],AGVName:"AGV_001",ScriptName:"script_name"}],selectedScheduleData:{},ShowScheduleDetail:!1}},methods:{GetRowIndex(e){return this.SecheulData.indexOf(e)},EditBtnClickHandler(){setTimeout((()=>{var e=q()(Date.now()).format("yyyy/MM/DD"),t=JSON.parse(JSON.stringify(this.selectedScheduleData));t.Time=e+" "+t.Time,this.$refs.schedule_edit_helper.EditorMode(t)}),.5)},async HandleDeleteScheduleClick(e){this.$swal.fire({html:`<b>時間:</b>${e.Time} <br/><b>AGV:</b>${e.AGVName} `,title:"確定要刪除此排程?",icon:"question",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((async t=>{t.isConfirmed&&(await(0,F.IG)(e.Time,e.AGVName),this.GetSchedulesFromBackend())}))},FormatTime(e,t,a,l){return q()(a).format("HH:mm")},async GetSchedulesFromBackend(){this.SecheulData=await(0,F.Fy)()},HandleSchedulesChanged(){this.GetSchedulesFromBackend()}},mounted(){this.GetSchedulesFromBackend()}};const J=(0,U.Z)(z,[["render",N],["__scopeId","data-v-b99541ea"]]);var K=J,j=a(71215),W={components:{AGVStatus:d.Z,TaskStatusVue:i.Z,Dispatcher:K,TaskAllocationVue:j.Z},mounted(){A.p.dispatch("DownloadMapData")}};const Q=(0,U.Z)(W,[["render",n]]);var X=Q}}]);
//# sourceMappingURL=740.1d9e846d.js.map