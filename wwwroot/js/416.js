"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[416],{92709:function(e,l,t){t.d(l,{LO:function(){return o},ji:function(){return a},uk:function(){return d}});const a={0:"TCP/Socket",1:"RESTFul API"},o={0:{label:"UNKNOWN",labelCN:"未知",color:"rgb(64, 158, 255)"},1:{label:"IDLE",labelCN:"閒置",color:"orange"},2:{label:"RUN",labelCN:"執行",color:"green"},3:{label:"DOWN",labelCN:"當機",color:"red"},4:{label:"Charging",labelCN:"充電",color:"#0d6efd"}},d={0:{label:"FORK",labelCN:"叉車",color:"rgb(64, 158, 255)"},1:{label:"YUNTECH-FORK",labelCN:"叉車(雲科)",color:"orange"},2:{label:"INSPECTOIN",labelCN:"巡檢",color:"green"},3:{label:"SUBMARINE",labelCN:"潛盾",color:"red"},4:{label:"PARTS",labelCN:"Parts",color:"blue"}}},67163:function(e,l,t){t.d(l,{Z:function(){return c}});var a=t(66252),o=t(3577);const d=["onMousemove"],r=["onMousemove"];function s(e,l,t,s,n,i){return(0,a.wg)(),(0,a.iD)("div",null,[(0,a._)("div",{class:"table-selector",onMouseleave:l[0]||(l[0]=()=>{n.selectedRows=n.selectedCols=0}),onMousemove:l[1]||(l[1]=(...l)=>e.handleMouseMove&&e.handleMouseMove(...l)),onClick:l[2]||(l[2]=(...e)=>i.handleClick&&i.handleClick(...e))},[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(t.maxColNum,(e=>((0,a.wg)(),(0,a.iD)("div",{key:e,onMousemove:l=>i.HandleMoseMoveOnRow(l,e)},[((0,a.wg)(!0),(0,a.iD)(a.HY,null,(0,a.Ko)(t.maxRowNum,(l=>((0,a.wg)(),(0,a.iD)("div",{class:(0,o.C_)(["cell d-flex",{selected:i.isSelected(e,l)}]),key:l,onMousemove:e=>i.HandleMoseMoveOnColumn(e,l)},null,42,r)))),128))],40,d)))),128))],32),(0,a._)("p",null,"選擇了 "+(0,o.zw)(n.selectedRows)+" 行 和 "+(0,o.zw)(n.selectedCols)+" 列 的表格",1)])}var n={props:{maxRowNum:{type:Number,default:6},maxColNum:{type:Number,default:10}},data(){return{selectedRows:0,selectedCols:0,hoverIndex:0}},methods:{isSelected(e,l){return l<=this.selectedRows&&e<=this.selectedCols},HandleMoseMoveOnRow(e,l){e.target;this.selectedCols=l},HandleMoseMoveOnColumn(e,l){e.target;this.selectedRows=l},handleClick(){this.$emit("on-selected",{row:this.selectedRows,col:this.selectedCols})}}},i=t(83744);const u=(0,i.Z)(n,[["render",s],["__scopeId","data-v-1578eb4a"]]);var c=u},69416:function(e,l,t){t.r(l),t.d(l,{default:function(){return D}});var a={};t.r(a);var o=t(66252);const d={class:"racks-manager"},r={class:""},s={class:""};function n(e,l,t,a,n,i){const u=(0,o.up)("RackListTable"),c=(0,o.up)("b-tab"),m=(0,o.up)("AddRack"),p=(0,o.up)("b-tabs"),w=(0,o.up)("b-card");return(0,o.wg)(),(0,o.iD)("div",d,[(0,o.Wm)(w,{"no-body":""},{default:(0,o.w5)((()=>[(0,o.Wm)(p,{pills:"",vertical:"",justified:"","nav-class":"my-nav","content-class":"my-nav-tabs"},{default:(0,o.w5)((()=>[(0,o.Wm)(c,{title:"RACK列表",active:""},{default:(0,o.w5)((()=>[(0,o._)("div",r,[(0,o.Wm)(u)])])),_:1}),(0,o.Wm)(c,{title:"新增RACK"},{default:(0,o.w5)((()=>[(0,o._)("div",s,[(0,o.Wm)(m)])])),_:1})])),_:1})])),_:1})])}var i=t(3577);const u=e=>((0,o.dD)("data-v-77d74552"),e=e(),(0,o.Cn)(),e),c={class:"add-rack d-flex w-100"},m={class:"d-flex flex-column"},p=u((()=>(0,o._)("div",{style:{width:"26px"}},[(0,o._)("i",{class:"bi bi-grid-3x3",style:{"font-size":"26px"}})],-1))),w={class:"border-top py-2 text-start"},f={class:"border-start bg-light rack-edit-preview"};function b(e,l,t,a,d,r){const s=(0,o.up)("el-divider"),n=(0,o.up)("el-input"),u=(0,o.up)("el-form-item"),b=(0,o.up)("el-option"),h=(0,o.up)("el-select"),V=(0,o.up)("el-input-number"),v=(0,o.up)("ColumnRowSelector"),y=(0,o.up)("el-popover"),_=(0,o.up)("el-switch"),C=(0,o.up)("el-form"),W=(0,o.up)("b-button"),k=(0,o.up)("Rack");return(0,o.wg)(),(0,o.iD)("div",c,[(0,o._)("div",null,[(0,o.Wm)(C,{"label-width":"100px","label-position":"left",style:{width:"400px"}},{default:(0,o.w5)((()=>[(0,o.Wm)(s,null,{default:(0,o.w5)((()=>[(0,o.Uk)("Basic")])),_:1}),(0,o.Wm)(u,{label:"RACK名稱"},{default:(0,o.w5)((()=>[(0,o.Wm)(n,{class:"add-rack-input",modelValue:d.payload.AGV_Name,"onUpdate:modelValue":l[0]||(l[0]=e=>d.payload.AGV_Name=e)},null,8,["modelValue"])])),_:1}),(0,o.Wm)(u,{label:"車輛類型"},{default:(0,o.w5)((()=>[(0,o.Wm)(h,{class:"add-rack-input",modelValue:d.payload.Model,"onUpdate:modelValue":l[1]||(l[1]=e=>d.payload.Model=e)},{default:(0,o.w5)((()=>[(0,o.Wm)(b,{label:"叉車 AGV",value:0}),(0,o.Wm)(b,{label:"巡檢 AGV",value:2}),(0,o.Wm)(b,{label:"潛盾 AGV",value:3}),(0,o.Wm)(b,{label:"Parts AGV",value:4})])),_:1},8,["modelValue"])])),_:1}),(0,o.Wm)(u,{label:"通訊方式"},{default:(0,o.w5)((()=>[(0,o.Wm)(h,{class:"add-rack-input",modelValue:d.payload.Protocol,"onUpdate:modelValue":l[2]||(l[2]=e=>d.payload.Protocol=e)},{default:(0,o.w5)((()=>[(0,o.Wm)(b,{label:"TCP/Socket",value:0}),(0,o.Wm)(b,{label:"RESTFul API",value:1})])),_:1},8,["modelValue"])])),_:1}),(0,o.Wm)(u,{label:"IP"},{default:(0,o.w5)((()=>[(0,o.Wm)(n,{class:"add-rack-input",modelValue:d.payload.IP,"onUpdate:modelValue":l[3]||(l[3]=e=>d.payload.IP=e)},null,8,["modelValue"])])),_:1}),(0,o.Wm)(u,{label:"Port"},{default:(0,o.w5)((()=>[(0,o.Wm)(n,{class:"add-rack-input",modelValue:d.payload.Port,"onUpdate:modelValue":l[4]||(l[4]=e=>d.payload.Port=e)},null,8,["modelValue"])])),_:1}),(0,o.Wm)(s,null,{default:(0,o.w5)((()=>[(0,o.Uk)("Layout")])),_:1}),(0,o.Wm)(u,{label:"Row"},{default:(0,o.w5)((()=>[(0,o.Wm)(V,{min:1,class:"add-rack-input",modelValue:d.payload.row,"onUpdate:modelValue":l[5]||(l[5]=e=>d.payload.row=e)},null,8,["modelValue"])])),_:1}),(0,o.Wm)(u,{label:"Col"},{default:(0,o.w5)((()=>[(0,o._)("div",m,[(0,o.Wm)(V,{min:1,class:"add-rack-input",modelValue:d.payload.col,"onUpdate:modelValue":l[6]||(l[6]=e=>d.payload.col=e)},null,8,["modelValue"]),(0,o.Wm)(y,{placement:"bottom-end",modelValue:d.colRowSelectorShow,"onUpdate:modelValue":l[8]||(l[8]=e=>d.colRowSelectorShow=e),width:"400",trigger:"click"},{reference:(0,o.w5)((()=>[p])),default:(0,o.w5)((()=>[(0,o.Wm)(v,{maxRowNum:3,onOnSelected:l[7]||(l[7]=l=>{this.colRowSelectorShow=e.fasle,this.payload.col=l.col,this.payload.row=l.row})})])),_:1},8,["modelValue"])])])),_:1}),(0,o.Wm)(s,null,{default:(0,o.w5)((()=>[(0,o.Uk)("Developer")])),_:1}),(0,o.Wm)(u,{label:"模擬"},{default:(0,o.w5)((()=>[(0,o.Wm)(_,{class:"add-rack-input",modelValue:d.payload.Simulation,"onUpdate:modelValue":l[9]||(l[9]=e=>d.payload.Simulation=e)},null,8,["modelValue"])])),_:1})])),_:1}),(0,o._)("div",w,[(0,o.Wm)(W,{onClick:l[10]||(l[10]=e=>r.IsEditMode?r.EditVehicle():r.AddVehicle()),variant:"primary",loading:d.adding,style:{width:"120px"}},{default:(0,o.w5)((()=>[(0,o.Uk)((0,i.zw)(r.btnText),1)])),_:1},8,["loading"])])]),(0,o._)("div",f,[(0,o.Uk)("Preview "),(0,o.Wm)(k,{rackName:"WIP-1",isEdit:!0,editProps:d.payload},null,8,["editProps"])])])}var h=t(38418),V=t(67163),v=t(8809),y={components:{Rack:v.Z,ColumnRowSelector:V.Z},props:{mode:{type:String,default:"add"}},computed:{IsEditMode(){return"edit"==this.mode},btnText(){return this.IsEditMode?"修改":"新增"}},data(){return{payload:{AGV_Name:"WIP-",Model:3,Protocol:0,IP:"127.0.0.1",Port:7025,row:3,col:3,Simulation:!1},oriAGVID:"",adding:!1,colRowSelectorShow:!1}},methods:{async AddVehicle(){this.adding=!0;var e=await h.fO.AddVehicle(this.payload);this.adding=!1,e.confirm?this.$swal.fire({text:"",title:"新增成功",icon:"success",showCancelButton:!1,confirmButtonText:"OK",customClass:"top-most-sweetalert"}):this.$swal.fire({text:e.message,title:"新增失敗",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"top-most-sweetalert"})},async EditVehicle(){var e=await h.fO.EditVehicle(this.payload,this.oriAGVID);e.confirm?this.$swal.fire({text:"",title:"修改成功",icon:"success",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}):this.$swal.fire({text:e.message,title:"修改失敗",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"})},UpdatePayload(e){this.payload=e,this.oriAGVID=e.AGV_Name}}},_=t(83744);const C=(0,_.Z)(y,[["render",b],["__scopeId","data-v-77d74552"]]);var W=C,k=t(49963);const A={class:"vehicle-list-table"},g={class:"text-start"},P={class:""};function x(e,l,t,a,d,r){const s=(0,o.up)("el-table-column"),n=(0,o.up)("el-tag"),u=(0,o.up)("el-checkbox"),c=(0,o.up)("el-button"),m=(0,o.up)("el-table"),p=(0,o.up)("AddRack"),w=(0,o.up)("el-drawer");return(0,o.wg)(),(0,o.iD)("div",A,[(0,o.Wm)(m,{"header-cell-class-name":"my-el-table-cell-class","header-row-class-name":"my-el-table-row-class","row-key":"AGV_Name",border:"",data:r.GetAGVStatesData,style:{width:"100%"}},{default:(0,o.w5)((()=>[(0,o.Wm)(s,{label:"AGV ID",prop:"AGV_Name"}),(0,o.Wm)(s,{label:"類型",prop:"Model"},{default:(0,o.w5)((e=>[(0,o.Wm)(n,{effect:"dark"},{default:(0,o.w5)((()=>[(0,o.Uk)((0,i.zw)(r.VehicleModels[e.row.Model].labelCN),1)])),_:2},1024)])),_:1}),(0,o.Wm)(s,{label:"當前狀態",prop:"MainStatus"},{default:(0,o.w5)((e=>[(0,o.Wm)(n,{effect:"dark",color:r.AGVMainStatus[e.row.MainStatus].color},{default:(0,o.w5)((()=>[(0,o.Uk)((0,i.zw)(r.AGVMainStatus[e.row.MainStatus].label),1)])),_:2},1032,["color"])])),_:1}),(0,o.Wm)(s,{label:"當前位置",prop:"CurrentLocation"}),(0,o.Wm)(s,{label:"通訊方式",prop:"Protocol"},{default:(0,o.w5)((e=>[(0,o.Wm)(n,null,{default:(0,o.w5)((()=>[(0,o.Uk)((0,i.zw)(r.ProtocolText[e.row.Protocol]),1)])),_:2},1024)])),_:1}),(0,o.Wm)(s,{label:"IP",prop:"IP"}),(0,o.Wm)(s,{label:"PORT",prop:"Port"}),(0,o.Wm)(s,{label:"車長(cm)",prop:"VehicleLength"}),(0,o.Wm)(s,{label:"車寬(cm)",prop:"VehicleWidth"}),(0,o.Wm)(s,{label:"啟用模擬",prop:"Simulation"},{default:(0,o.w5)((e=>[(0,o.Wm)(u,{disabled:!0,modelValue:e.row.Simulation,"onUpdate:modelValue":l=>e.row.Simulation=l},null,8,["modelValue","onUpdate:modelValue"])])),_:1}),(0,o.Wm)(s,{fixed:"right",label:"Operations",width:"160"},{default:(0,o.w5)((e=>[(0,o.Wm)(c,{type:"success",size:"small",onClick:(0,k.iM)((l=>r.edit_row(e.row)),["prevent"])},{default:(0,o.w5)((()=>[(0,o.Uk)(" 編輯 ")])),_:2},1032,["onClick"]),(0,o.Wm)(c,{type:"danger",size:"small",onClick:(0,k.iM)((l=>r.delete_row(e.row)),["prevent"])},{default:(0,o.w5)((()=>[(0,o.Uk)(" 刪除 ")])),_:2},1032,["onClick"])])),_:1})])),_:1},8,["data"]),(0,o.Wm)(w,{"z-index":"1",modelValue:d.ShowEditAGVPropertyDrawer,"onUpdate:modelValue":l[0]||(l[0]=e=>d.ShowEditAGVPropertyDrawer=e)},{header:(0,o.w5)((({})=>[(0,o._)("h3",g,(0,i.zw)(r.drawerText),1)])),default:(0,o.w5)((()=>[(0,o._)("div",P,[(0,o.Wm)(p,{ref:"AgvPropertyEditor",mode:"edit"},null,512)])])),_:1},8,["modelValue"])])}var S=t(24239),M=t(92709),N={components:{AddRack:W},data(){return{table:[],selectAGVProertyToEdit:{},ShowEditAGVPropertyDrawer:!1}},computed:{GetAGVStatesData(){return S.sn.getters.AGVStatesData},ProtocolText(){return M.ji},AGVMainStatus(){return M.LO},VehicleModels(){return M.uk},drawerText(){return this.selectAGVProertyToEdit.AGV_Name}},methods:{edit_row(e){this.selectAGVProertyToEdit=e,this.ShowEditAGVPropertyDrawer=!0,setTimeout((()=>{this.$refs["AgvPropertyEditor"].UpdatePayload(e)}),1)},async delete_row(e){var l=async()=>{var l=await h.fO.DeleteVehicle(e.AGV_Name);l.confirm?this.$swal.fire({text:"",title:"刪除車輛成功",icon:"success",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}):this.$swal.fire({text:l.message,title:"刪除車輛失敗",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"})};this.$swal.fire({text:"",title:`確定要刪除車輛-${e.AGV_Name}?`,icon:"warning",showCancelButton:!0,confirmButtonText:"OK",customClass:"my-sweetalert"}).then((e=>{e.isConfirmed&&l()}))}}};const R=(0,_.Z)(N,[["render",x]]);var G=R,U={components:{AddRack:W,RackListTable:G},data(){return{test:"AV"}}};"function"===typeof a["default"]&&(0,a["default"])(U);const T=(0,_.Z)(U,[["render",n]]);var D=T}}]);
//# sourceMappingURL=416.js.map