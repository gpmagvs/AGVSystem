"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[121],{91583:function(e,t,o){o.r(t),o.d(t,{default:function(){return _}});var a=o(66252);const s=e=>((0,a.dD)("data-v-20d03ca6"),e=e(),(0,a.Cn)(),e),l={class:"bg-light h-100 d-flex"},n={class:"tab-container"},r={class:"p-2 d-flex bg-light border-bottom"},i={class:"w-100"},u=s((()=>(0,a._)("h3",{class:"text-start text-danger border-bottom my-3"},"SECS Config設定",-1))),d={class:"tab-container"},c={class:"p-2 d-flex bg-light border-bottom"},g={class:"w-100"},C=s((()=>(0,a._)("h3",{class:"text-start text-danger border-bottom my-3"},"Transfer Complete Result Code",-1)));function f(e,t,o,s,f,m){const p=(0,a.up)("el-button"),w=(0,a.up)("el-input"),h=(0,a.up)("el-form-item"),b=(0,a.up)("el-form"),R=(0,a.up)("el-col"),D=(0,a.up)("el-row"),y=(0,a.up)("b-tab"),v=(0,a.up)("el-input-number"),k=(0,a.up)("b-tabs"),S=(0,a.Q2)("loading");return(0,a.wy)(((0,a.wg)(),(0,a.iD)("div",l,[(0,a.Wm)(k,{class:"w-100",modelValue:f.tabSelected,"onUpdate:modelValue":t[2]||(t[2]=e=>f.tabSelected=e)},{default:(0,a.w5)((()=>[(0,a.Wm)(y,{class:"",title:"Basic"},{default:(0,a.w5)((()=>[(0,a._)("div",n,[(0,a._)("div",r,[(0,a.Wm)(p,{size:"large",type:"primary",onClick:m.SECSConfigHandleSaveButtonClicked},{default:(0,a.w5)((()=>[(0,a.Uk)("儲存")])),_:1},8,["onClick"]),(0,a.Wm)(p,{size:"large",onClick:t[0]||(t[0]=()=>{m.DownloadConfigurations()})},{default:(0,a.w5)((()=>[(0,a.Uk)("重新載入")])),_:1})]),(0,a.Wm)(D,{class:"m-3"},{default:(0,a.w5)((()=>[(0,a.Wm)(R,{lg:8,class:"border px-5"},{default:(0,a.w5)((()=>[(0,a._)("div",i,[u,(0,a.Wm)(b,{"label-position":"left","label-width":"320px",style:{"max-height":"70vh","overflow-y":"auto"}},{default:(0,a.w5)((()=>[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(f.configuration.baseConfiguration,((t,o)=>((0,a.wg)(),(0,a.j4)(h,{key:o,label:"-"+e.$t(`${o}`)},{default:(0,a.w5)((()=>[(0,a.Wm)(w,{modelValue:f.configuration.baseConfiguration[o],"onUpdate:modelValue":e=>f.configuration.baseConfiguration[o]=e},null,8,["modelValue","onUpdate:modelValue"])])),_:2},1032,["label"])))),128))])),_:1})])])),_:1}),(0,a.Wm)(R,{lg:12})])),_:1})])])),_:1}),(0,a.Wm)(y,{class:"",title:"Return Code 設定"},{default:(0,a.w5)((()=>[(0,a._)("div",d,[(0,a._)("div",c,[(0,a.Wm)(p,{size:"large",type:"primary",onClick:m.HandleSaveButtonClicked},{default:(0,a.w5)((()=>[(0,a.Uk)("儲存")])),_:1},8,["onClick"]),(0,a.Wm)(p,{size:"large",onClick:t[1]||(t[1]=()=>{m.DownloadConfigurations()})},{default:(0,a.w5)((()=>[(0,a.Uk)("重新載入")])),_:1})]),(0,a.Wm)(D,{class:"m-3"},{default:(0,a.w5)((()=>[(0,a.Wm)(R,{lg:8,class:"border px-5"},{default:(0,a.w5)((()=>[(0,a._)("div",g,[C,(0,a.Wm)(b,{"label-position":"left","label-width":"320px",style:{"max-height":"70vh","overflow-y":"auto"}},{default:(0,a.w5)((()=>[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(f.configuration.transferReportConfiguration.ResultCodes,((t,o)=>((0,a.wg)(),(0,a.j4)(h,{key:o,label:"-"+e.$t(`secsGem.${o.replace("ResultCode","")}`)},{default:(0,a.w5)((()=>[(0,a.Wm)(v,{modelValue:f.configuration.transferReportConfiguration.ResultCodes[o],"onUpdate:modelValue":e=>f.configuration.transferReportConfiguration.ResultCodes[o]=e,min:0,max:999,controls:!1},null,8,["modelValue","onUpdate:modelValue"])])),_:2},1032,["label"])))),128))])),_:1})])])),_:1}),(0,a.Wm)(R,{lg:12})])),_:1})])])),_:1})])),_:1},8,["modelValue"]),(0,a.kq)("",!0)])),[[S,f.loading]])}var m=o(9669),p=o.n(m),w=o(663),h=p().create({baseURL:w.Z.backend_host});async function b(){try{var e=await h.get("/api/SecsGemConfiguration/");return e.data}catch(t){throw t}}async function R(e={transferCompletedResultCodes:{}}){var t=await h.post("/api/SecsGemConfiguration/saveReturnCodeSetting",e);return t.data}async function D(e={SECSConfig:{}}){var t=await h.post("/api/SecsGemConfiguration/saveSECSGemSetting",e);return t.data}var y=o(49996),v={data(){return{loading:!0,tabSelected:0,route:"",configuration:{baseConfiguration:{DeviceID:"2F_AGVC02",CarrierLOCPrefixName:"AOIRACK001",SystemID:"022",UnknowTrayIDFlowNumberUsed:135,UnknowRackIDFlowNumberUsed:1,DoubleUnknowDFlowNumberUsed:240,DoubleUnknowRackIDFlowNumberUsed:0,MissMatchTrayIDFlowNumberUsed:1,MissMatchRackIDFlowNumberUsed:0},alarmConfiguration:{Version:0},transferReportConfiguration:{ResultCodes:{OtherErrorsResultCode:1,ZoneIsFullResultCode:1,UnloadButCargoIDReadNotMatchedResultCode:1,UnloadButCargoIDReadFailResultCode:1,InterlockErrorResultCode:1,EqUnloadButNoCargoResultCode:1,AGVDownWhenLDULDWithCargoResultCode:1,AGVDownWhenLDWithoutCargoResultCode:1,AGVDownWhenULDWithoutCargoResultCode:1,AGVDownWhenMovingToDestineResultCode:1,DestineEqLoadReqeustOff:1,DestineEqHasCargoResultCode:1,DestineEqMachechStateErrorResultCode:1,DestineEqDownResultCode:1,SourceEqUnloadReqeustOff:1,SourceEqNotHasCargoResultCode:1,SourceEqMachechStateErrorResultCode:1,SourceEqDownResultCode:1,DestineRackPortHasCargoResultCode:1,SourceRackPortNotHasCargoResultCode:1}},alarmConfigFilePath:"",transferReportConfigFilePath:""}}},mounted(){this.route=this.$route.path,this.DownloadConfigurations()},methods:{async DownloadConfigurations(){this.loading=!0;try{this.configuration=await b()}catch(e){alert(e.message)}finally{setTimeout((()=>{this.loading=!1}),300)}},async HandleSaveButtonClicked(){try{const e=this.configuration.transferReportConfiguration.ResultCodes,t=this.ShowRepeatedResultCode(e);if(t.hasDuplicates)return void this.$swal.fire({text:`有重複的Result Code: ${t.duplicateValues.join(", ")}`,title:"",icon:"warning",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"});let o=await R({transferCompletedResultCodes:this.configuration.transferReportConfiguration.ResultCodes});if(!o)return void(0,y.bM)({message:"儲存失敗",type:"error"});o.confirm?(0,y.bM)({message:"儲存成功",type:"success"}):(0,y.bM)({message:"儲存失敗-"+o.message,type:"success"})}catch(e){(0,y.bM)({message:"儲存失敗-"+e.message,type:"success"})}},async SECSConfigHandleSaveButtonClicked(){try{let e=await D(this.configuration.baseConfiguration);if(!e)return void(0,y.bM)({message:"儲存失敗",type:"error"});e.confirm?(0,y.bM)({message:"儲存成功",type:"success"}):(0,y.bM)({message:"儲存失敗-"+e.message,type:"success"})}catch(e){(0,y.bM)({message:"儲存失敗-"+e.message,type:"success"})}},ShowRepeatedResultCode(e){const t=Object.values(e);console.log("All Result Codes:",t);const o=new Map;t.forEach((e=>{o.set(e,(o.get(e)||0)+1)}));const a=Array.from(o.entries()).filter((([e,t])=>t>1)).map((([e])=>e));return console.log("Duplicate Result Codes:",a),{hasDuplicates:a.length>0,duplicateValues:a}}},watch:{$route(e,t){e.path!=this.route||console.log("Route changed:",e.path)}}},k=o(83744);const S=(0,k.Z)(v,[["render",f],["__scopeId","data-v-20d03ca6"]]);var _=S}}]);
//# sourceMappingURL=121.873155a0.js.map