"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[943],{27943:function(e,t,a){a.r(t),a.d(t,{default:function(){return X}});var l=a(66252);const s={class:"d-flex"},i={class:"actions w-50"};function d(e,t,a,d,n,o){const u=(0,l.up)("AGVStatus"),c=(0,l.up)("Dispatcher"),r=(0,l.up)("task-status-vue"),m=(0,l.up)("TaskAllocationVue");return(0,l.wg)(),(0,l.iD)("div",null,[(0,l._)("div",s,[(0,l._)("div",i,[(0,l.Wm)(u),(0,l.Wm)(c)]),(0,l.Wm)(r,{class:"flex-fill",show_card_title:!1,display_mode:"column"})]),(0,l.Wm)(m)])}var n=a(3802),o=a(61130),u=a(3577);const c=e=>((0,l.dD)("data-v-b99541ea"),e=e(),(0,l.Cn)(),e),r={class:"dispatcher border rounded p-2"},m={class:"d-flex p-2 border"},h={class:"border mx-2"},p={class:"bg-primary text-light w-100"},_=c((()=>(0,l._)("div",{class:"p-2 border"},null,-1)));function w(e,t,a,s,i,d){const n=(0,l.up)("b-button"),o=(0,l.up)("el-table-column"),c=(0,l.up)("el-button"),w=(0,l.up)("el-table"),b=(0,l.up)("b-tab"),g=(0,l.up)("b-tabs"),y=(0,l.up)("NewScheduleHelper");return(0,l.wg)(),(0,l.iD)("div",r,[(0,l.Wm)(g,null,{default:(0,l.w5)((()=>[(0,l.Wm)(b,{title:"排程派車"},{default:(0,l.w5)((()=>[(0,l._)("div",m,[(0,l._)("div",null,[(0,l.Wm)(n,{onClick:t[0]||(t[0]=()=>{e.$refs.schedule_add_helper.Show()}),variant:"primary"},{default:(0,l.w5)((()=>[(0,l.Uk)("新增排程")])),_:1})]),(0,l._)("div",h,[(0,l._)("label",p,"排程列表("+(0,u.zw)(i.SecheulData.length)+")",1),(0,l.Wm)(w,{style:{height:"350px"},data:i.SecheulData,"highlight-current-row":"",onRowClick:t[1]||(t[1]=(e,t,a)=>{i.selectedScheduleData=e})},{default:(0,l.w5)((()=>[(0,l.Wm)(o,{label:"時間","min-width":"160",prop:"Time"}),(0,l.Wm)(o,{label:"車輛","min-width":"160",prop:"AGVName"}),(0,l.Wm)(o,{label:"Bay數量","min-width":"160",prop:"Bays"},{default:(0,l.w5)((e=>[(0,l.Uk)((0,u.zw)(e.row.Bays.length),1)])),_:1}),(0,l.Wm)(o,{label:"操作","min-width":"150"},{default:(0,l.w5)((e=>[(0,l.Wm)(c,{size:"small",type:"primary",onClick:d.EditBtnClickHandler},{default:(0,l.w5)((()=>[(0,l.Uk)("檢視")])),_:1},8,["onClick"]),(0,l.Wm)(c,{size:"small",type:"danger",onClick:t=>d.HandleDeleteScheduleClick(e.row)},{default:(0,l.w5)((()=>[(0,l.Uk)("刪除")])),_:2},1032,["onClick"])])),_:1})])),_:1},8,["data"])])])])),_:1}),(0,l.Wm)(b,{title:"即時派車"},{default:(0,l.w5)((()=>[_])),_:1})])),_:1}),(0,l.Wm)(y,{onOn_submit:d.HandleSchedulesChanged,ref:"schedule_add_helper"},null,8,["onOn_submit"]),(0,l.Wm)(y,{onOn_submit:d.HandleSchedulesChanged,edit_mode:!0,ref:"schedule_edit_helper"},null,8,["onOn_submit"])])}const b=e=>((0,l.dD)("data-v-b05f48d2"),e=e(),(0,l.Cn)(),e),g={class:"login-header"},y=["id"],f={class:"copntent"},S={class:"d-flex text-start p-1"},v={class:"options border p-2",style:{width:"700px"}},B={class:"border rounded p-2 my-2"},C=b((()=>(0,l._)("div",{class:"label"},[(0,l._)("i",{class:"bi bi-clock"}),(0,l.Uk)("時間設定")],-1))),k={class:"border rounded p-2 my-2"},D=b((()=>(0,l._)("div",{class:"label"},[(0,l._)("i",{class:"bi bi-truck-front"}),(0,l.Uk)("指派車輛")],-1))),N={class:"border rounded p-2 my-2"},T=b((()=>(0,l._)("div",{class:"label"},[(0,l._)("i",{class:"bi bi-geo"}),(0,l.Uk)("量測Bay選取")],-1))),V=b((()=>(0,l._)("div",{class:"d-flex flex-column"},null,-1))),A={class:"",style:{height:"350px"}};function W(e,t,a,s,i,d){const n=(0,l.up)("el-divider"),o=(0,l.up)("el-time-picker"),c=(0,l.up)("el-option"),r=(0,l.up)("el-select"),m=(0,l.up)("el-table-column"),h=(0,l.up)("el-table"),p=(0,l.up)("b-button"),_=(0,l.up)("Map"),w=(0,l.up)("el-dialog");return(0,l.wg)(),(0,l.j4)(w,{modelValue:i.ShowDialog,"onUpdate:modelValue":t[2]||(t[2]=e=>i.ShowDialog=e),"show-close":!0,"close-on-click-modal":!0,"close-on-press-escape":!0,modal:!1,draggable:"",width:"800",fullscreen:"",style:{"z-index":"29900"}},{header:(0,l.w5)((({titleId:e,login_title:t})=>[(0,l._)("div",g,[(0,l._)("h3",{id:e,class:(0,u.C_)(t)},(0,u.zw)(i.Title),11,y),(0,l.Wm)(n)])])),default:(0,l.w5)((()=>[(0,l._)("div",f,[(0,l._)("div",S,[(0,l._)("div",v,[(0,l._)("div",B,[C,(0,l.Wm)(o,{format:"HH:mm",modelValue:i.schedule_settigs.Time,"onUpdate:modelValue":t[0]||(t[0]=e=>i.schedule_settigs.Time=e),type:"time",placeholder:"選擇量測時間"},null,8,["modelValue"])]),(0,l._)("div",k,[D,(0,l.Wm)(r,{modelValue:i.schedule_settigs.AGVName,"onUpdate:modelValue":t[1]||(t[1]=e=>i.schedule_settigs.AGVName=e),placeholder:"選擇車輛",style:{width:"220px"}},{default:(0,l.w5)((()=>[((0,l.wg)(!0),(0,l.iD)(l.HY,null,(0,l.Ko)(d.AgvNameList,(e=>((0,l.wg)(),(0,l.j4)(c,{key:e,label:e,value:e},null,8,["label","value"])))),128))])),_:1},8,["modelValue"])]),(0,l._)("div",N,[T,V,(0,l._)("div",null,[(0,l._)("div",A,[(0,l.Wm)(h,{border:"",data:i.BayTableData,ref:"table_ref",onSelectionChange:d.handleSelectionChange},{default:(0,l.w5)((()=>[(0,l.Wm)(m,{type:"selection",label:"選取"}),(0,l.Wm)(m,{prop:"BayName",label:"Bay名稱",width:"100"}),(0,l.Wm)(m,{prop:"BayName",label:"量測點",width:"400"},{default:(0,l.w5)((e=>[(0,l.Wm)(r,{onClick:t=>d.HandlePointsColumnClick(e.row),modelValue:e.row.SelectedPointNames,"onUpdate:modelValue":t=>e.row.SelectedPointNames=t,multiple:"",style:{width:"100%"},onChange:d.HandlePointsSelectedChange},{default:(0,l.w5)((()=>[((0,l.wg)(!0),(0,l.iD)(l.HY,null,(0,l.Ko)(e.row.PointNames,(e=>((0,l.wg)(),(0,l.j4)(c,{key:e,label:e,value:e},null,8,["label","value"])))),128))])),_:2},1032,["onClick","modelValue","onUpdate:modelValue","onChange"])])),_:1}),(0,l.Wm)(m,{label:"量測順序"},{default:(0,l.w5)((e=>[(0,l.Wm)(r,{modelValue:e.row.Sequence,"onUpdate:modelValue":t=>e.row.Sequence=t,onClick:t=>d.HandlePointsColumnClick(e.row),onChange:d.HandleSequenceChange},{default:(0,l.w5)((()=>[((0,l.wg)(!0),(0,l.iD)(l.HY,null,(0,l.Ko)(i.SequenceList,(e=>((0,l.wg)(),(0,l.j4)(c,{key:e,label:e,value:e},null,8,["label","value"])))),128))])),_:2},1032,["modelValue","onUpdate:modelValue","onClick","onChange"])])),_:1})])),_:1},8,["data","onSelectionChange"])])])]),(0,l.Wm)(p,{disabled:d.IsNoBaySelected||""==i.schedule_settigs.AGVName||""==i.schedule_settigs.Time,onClick:d.HandleAddNewScheduleClick,style:{cursor:"pointer"},class:"w-100 my-3",variant:"primary"},{default:(0,l.w5)((()=>[(0,l.Uk)((0,u.zw)(a.edit_mode?"修改":"新增排程"),1)])),_:1},8,["disabled","onClick"])]),(0,l.Wm)(_,{class:"bg-light border rounded px-1 mx-1",id:"schedule_map",agv_show:!1,style:{"padding-right":"10px",width:"900px"}})])])])),_:1},8,["modelValue"])}a(57658);var H=a(60351),G=a(95320),x=a(25044),q=a(30381),U=a.n(q),P=a(9669),$=a.n(P),O=a(663),I=$().create({baseURL:O.Z.backend_host});async function M(){var e=await I.get("/api/InstrumentMeasure/GetBaysTableData");return e.data}var L=a(3567),z={components:{Map:H["default"]},props:{edit_mode:{type:Boolean,default:!1}},data(){return{ShowDialog:!1,Title:"新增量測排程",region_mode:"局部選擇",selected_row:{},previous_seq:0,modified:{Time:"",AGVName:""},schedule_settigs:{Time:"1991/12/20 12:10:20",Bays:[],AGVName:"AGV_001",ScriptName:"script_name"},BayTableData:[{BayName:"Bay1",PointNames:["AAA12","2","3"],SelectedPointNames:["AAA12","3"],Sequence:1},{BayName:"Bay2",PointNames:["1","2","3"],SelectedPointNames:["1","3"],Sequence:2}],SequenceList:[]}},computed:{map_station_data(){return G.p.getters.MapStations},BaysData(){return G.p.getters.BaysData},BayNames(){return Object.keys(G.p.getters.BaysData)},IsAllSeclted(){return this.BayNames.length==this.schedule_settigs.Bays.length|0==this.schedule_settigs.Bays.length},AgvNameList(){return x.sn.getters.AGVNameList},IsNoBaySelected(){return this.BayTableData.every((e=>0==e.Selected))}},methods:{async Show(){this.ShowDialog=!0,this.CreateDafualt(!0)},EditorMode(e){this.CreateDafualt(!1);var t=U()(e.Time).format("HH:mm");this.Title=`排程量測 - ${t}`,this.modified.Time=t,this.modified.AGVName=e.AGVName,this.schedule_settigs=e,setTimeout((()=>{var t=e.Bays.map((e=>e.BayName));this.BayTableData.forEach((a=>{var l=t.includes(a.BayName);if(a.Selected=l,l){var s=e.Bays.find((e=>e.BayName==a.BayName));a.SelectedPointNames=s.SelectedPointNames,a.Sequence=s.Sequence}this.$refs["table_ref"].toggleRowSelection(a,l)}))}),420),this.ShowDialog=!0},CreateDafualt(e){M().then((t=>{this.BayTableData=t,this.SequenceList=[];var a=this.BayTableData.length;for(let e=0;e<a;e++)this.SequenceList.push(e+1);setTimeout((()=>{this.BayTableData.forEach((t=>{t&&(t.Selected=e,this.$refs["table_ref"].toggleRowSelection(t,e))}))}),200)}))},HandlePointsColumnClick(e){this.selected_row=e,this.previous_seq=e.Sequence},async HandleAddNewScheduleClick(){this.schedule_settigs.Bays=this.BayTableData.filter((e=>e.Selected));var e=JSON.parse(JSON.stringify(this.schedule_settigs));e.Time=U()(e.Time).format("HH:mm"),this.ShowDialog=!1,this.$swal.fire({html:`<b>時間:</b>${e.Time} <br/><b>AGV:</b>${e.AGVName} <br/><b>量測Bay數量:</b>${e.Bays.length}`,title:this.edit_mode?"確定要修改此排程":"確定要新增此排程?",icon:"question",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((async t=>{if(t.isConfirmed){var a={result:!1,message:""};a=this.edit_mode?await(0,L.x0)(this.modified.Time,this.modified.AGVName,e):await(0,L.Wp)(e),this.$swal.fire({text:"",title:this.edit_mode?a.result?"修改成功":"修改失敗":a.result?"新增成功":"新增失敗",icon:a.result?"success":"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((()=>{this.ShowDialog=!0})),this.$emit("on_submit","")}else this.ShowDialog=!0}))},HandlePointsSelectedChange(e){this.selected_row.SelectedPointNames=e},HandleSequenceChange(e){var t=this.BayTableData.find((t=>t.Sequence==e));t.Sequence=this.previous_seq,this.selected_row.Sequence=e},handleSelectionChange(e){var t=e.map((e=>e.BayName));this.BayTableData.forEach((e=>{e.Selected=t.includes(e.BayName)}))}},mounted(){}},E=a(83744);const Z=(0,E.Z)(z,[["render",W],["__scopeId","data-v-b05f48d2"]]);var F=Z,K={components:{NewScheduleHelper:F},data(){return{SecheulData:[{Time:"1991/12/20 12:10:20",Bays:[],AGVName:"AGV_001",ScriptName:"script_name"}],selectedScheduleData:{},ShowScheduleDetail:!1}},methods:{GetRowIndex(e){return this.SecheulData.indexOf(e)},EditBtnClickHandler(){setTimeout((()=>{var e=U()(Date.now()).format("yyyy/MM/DD"),t=JSON.parse(JSON.stringify(this.selectedScheduleData));t.Time=e+" "+t.Time,this.$refs.schedule_edit_helper.EditorMode(t)}),.5)},async HandleDeleteScheduleClick(e){this.$swal.fire({html:`<b>時間:</b>${e.Time} <br/><b>AGV:</b>${e.AGVName} `,title:"確定要刪除此排程?",icon:"question",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((async t=>{t.isConfirmed&&(await(0,L.IG)(e.Time,e.AGVName),this.GetSchedulesFromBackend())}))},FormatTime(e,t,a,l){return U()(a).format("HH:mm")},async GetSchedulesFromBackend(){this.SecheulData=await(0,L.Fy)()},HandleSchedulesChanged(){this.GetSchedulesFromBackend()}},mounted(){this.GetSchedulesFromBackend()}};const j=(0,E.Z)(K,[["render",w],["__scopeId","data-v-b99541ea"]]);var R=j,J=a(44883),Y={components:{AGVStatus:n.Z,TaskStatusVue:o.Z,Dispatcher:R,TaskAllocationVue:J.Z},mounted(){G.p.dispatch("DownloadMapData")}};const Q=(0,E.Z)(Y,[["render",d]]);var X=Q}}]);
//# sourceMappingURL=943.071311fb.js.map