"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[214],{14214:function(t,a,e){e.r(a),e.d(a,{default:function(){return v}});var n=e(66252);const s=t=>((0,n.dD)("data-v-b16ccc60"),t=t(),(0,n.Cn)(),t),r={class:"map-view h-100 d-flex flex-row my-1"},o=s((()=>(0,n._)("div",null,null,-1)));function i(t,a,e,s,i,p){const l=(0,n.up)("Map");return(0,n.wg)(),(0,n.iD)("div",r,[o,(0,n.Wm)(l,{id:"editable_map",agv_upload_coordi_data:p.agv_upload_data,onSave:p.SaveMapClickHandle,editable:!0,agv_show:!0,canva_height:"750px",ref:"map_editing"},null,8,["agv_upload_coordi_data","onSave"])])}e(57658);var p=e(67075),l=e(83592),c=e(93867),m=e(53259),u=e(95320),g=e(24239),h={components:{Map:p["default"]},async mounted(){this.tags=await l.ZP.GetMapTags()},data(){return{path_plan_point_type:"Tag",path_plan_point_from:1,path_plan_point_to:2,tags:[1,2,3,59,11],map_saving:!1}},computed:{StyleBiding(){var t=g.g4.getters.SystemAlarmShowState,a=g.g4.getters.EqpAlarmShowState;return t||a?t&&a?{top:"50px"}:{top:"12px"}:{top:"-10px"}},IsEditable(){return"編輯"==this.mode_selected},map_station_data(){return u.p.getters.MapStations},agv_upload_data(){return u.p.getters.AGVLocUpload},loadingText(){return"圖資儲存中..."}},methods:{async SaveMapClickHandle(t){console.log(t);var a=JSON.parse(JSON.stringify(u.p.getters.MapData));a.Points=t.Points,a.Segments=t.Pathes,a.Regions=t.Regions,a.Map_Image_Boundary=t.ImageExtent;var e=this.CheckMapContentHasAnyError(a);if(e.correct){this.map_saving=!0,this.$swal.fire({text:"",title:"圖資儲存中...",icon:"warning",showCancelButton:!1,showConfirmButton:!1,customClass:"my-sweetalert"});var n=await u.p.dispatch("SaveMap",{data:a,file:t.ImageFile});this.map_saving=!1,n?(m.Z.emit("/map_save"),this.$swal.fire({title:"圖資儲存成功",icon:"success",showCancelButton:!1,showConfirmButton:!1,customClass:"my-sweetalert",timer:1500})):(c.Z.Danger("圖資儲存失敗"),this.$swal.fire({title:"圖資儲存失敗",icon:"error",showCancelButton:!1,showConfirmButton:!0,customClass:"my-sweetalert"}))}else this.$swal.fire({text:e.message,title:"",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"})},CheckMapContentHasAnyError(t){var a=Object.values(t.Points).map((t=>t.TagNumber)),e=[...new Set(a)];if(e.length!=a.length){const e=[],s={};a.forEach(((t,a)=>{s.hasOwnProperty(t)?e.push(t):s[t]=a}));var n=[...new Set(e)].map((a=>{var e=Object.values(t.Points).filter((t=>t.TagNumber==a));return`Tag-${a}於 ${e.map((t=>t.Graph.Display)).join("、")}重複設置`}));return{correct:!1,message:`${n.join(";")} ，\n請再檢查圖資設定`}}return{correct:!0,message:""}},async GetPathPlanedFromServer(){var t={};if("Tag"==this.path_plan_point_type&&(t=await l.ZP.PathPlanByTag(this.path_plan_point_from,this.path_plan_point_to)),t){var a=t.tags;this.$refs["map"].UpdatePathPlanRender(a)}}},mounted(){}},_=e(83744);const d=(0,_.Z)(h,[["render",i],["__scopeId","data-v-b16ccc60"]]);var v=d}}]);
//# sourceMappingURL=214.e5afe93b.js.map