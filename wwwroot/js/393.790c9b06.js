"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[393],{49393:function(a,t,e){e.r(t),e.d(t,{default:function(){return v}});var n=e(66252);const s=a=>((0,n.dD)("data-v-52842129"),a=a(),(0,n.Cn)(),a),r={class:"map-view h-100 d-flex flex-row my-1"},o=s((()=>(0,n._)("div",null,null,-1)));function i(a,t,e,s,i,p){const l=(0,n.up)("Map");return(0,n.wg)(),(0,n.iD)("div",r,[o,(0,n.Wm)(l,{id:"editable_map",agv_upload_coordi_data:p.agv_upload_data,onSave:p.SaveMapClickHandle,onTempSave:p.TempSaveHandler,editable:!0,agv_show:!0,canva_height:"750px",ref:"map_editing"},null,8,["agv_upload_coordi_data","onSave","onTempSave"])])}e(57658);var p=e(74800),l=e(83592),m=e(93867),c=e(53259),d=e(95320),h=e(25044),g={components:{Map:p["default"]},async mounted(){this.tags=await l.ZP.GetMapTags()},data(){return{path_plan_point_type:"Tag",path_plan_point_from:1,path_plan_point_to:2,tags:[1,2,3,59,11],map_saving:!1}},computed:{StyleBiding(){var a=h.g4.getters.SystemAlarmShowState,t=h.g4.getters.EqpAlarmShowState;return a||t?a&&t?{top:"50px"}:{top:"12px"}:{top:"-10px"}},IsEditable(){return"編輯"==this.mode_selected},map_station_data(){return d.p.getters.MapStations},agv_upload_data(){return d.p.getters.AGVLocUpload},loadingText(){return"圖資儲存中..."}},methods:{async TempSaveHandler(a){var t=JSON.parse(JSON.stringify(d.p.getters.MapData));this.UpdateMapData(t,a),await d.p.dispatch("AddMapDataCache",t)},async SaveMapClickHandle(a){var t=JSON.parse(JSON.stringify(d.p.getters.MapData));this.UpdateMapData(t,a);var e=this.CheckMapContentHasAnyError(t);if(e.correct){this.map_saving=!0,this.$swal.fire({text:"",title:"圖資儲存中...",icon:"warning",showCancelButton:!1,showConfirmButton:!1,customClass:"my-sweetalert"});var n=await d.p.dispatch("SaveMap",{data:t,file:a.ImageFile});this.map_saving=!1,n?(c.Z.emit("/map_save"),this.$swal.fire({title:"圖資儲存成功",icon:"success",showCancelButton:!1,showConfirmButton:!1,customClass:"my-sweetalert",timer:1500})):(m.Z.Danger("圖資儲存失敗"),this.$swal.fire({title:"圖資儲存失敗",icon:"error",showCancelButton:!1,showConfirmButton:!0,customClass:"my-sweetalert"}))}else this.$swal.fire({text:e.message,title:"",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"})},UpdateMapData(a,t){a.Points=t.Points,a.Segments=t.Pathes,a.Regions=t.Regions,a.Map_Image_Boundary=t.ImageExtent},CheckMapContentHasAnyError(a){var t=Object.values(a.Points).map((a=>a.TagNumber)),e=[...new Set(t)];if(e.length!=t.length){const e=[],s={};t.forEach(((a,t)=>{s.hasOwnProperty(a)?e.push(a):s[a]=t}));var n=[...new Set(e)].map((t=>{var e=Object.values(a.Points).filter((a=>a.TagNumber==t));return`Tag-${t}於 ${e.map((a=>a.Graph.Display)).join("、")}重複設置`}));return{correct:!1,message:`${n.join(";")} ，\n請再檢查圖資設定`}}return{correct:!0,message:""}},async GetPathPlanedFromServer(){var a={};if("Tag"==this.path_plan_point_type&&(a=await l.ZP.PathPlanByTag(this.path_plan_point_from,this.path_plan_point_to)),a){var t=a.tags;this.$refs["map"].UpdatePathPlanRender(t)}}},mounted(){}},u=e(83744);const _=(0,u.Z)(g,[["render",i],["__scopeId","data-v-52842129"]]);var v=_}}]);
//# sourceMappingURL=393.790c9b06.js.map