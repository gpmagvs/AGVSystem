"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[15],{8015:function(t,e,r){r.r(e),r.d(e,{default:function(){return it}});var s=r(66252);const o={class:"rack-status-view custom-tabs-head p-1"},a={class:"rack-container d-flex flex-row justify-content-center border"};function i(t,e,r,i,n,l){const c=(0,s.up)("RackStatus"),u=(0,s.up)("b-tab"),d=(0,s.up)("b-tabs");return(0,s.wg)(),(0,s.iD)("div",o,[(0,s.Wm)(d,null,{default:(0,s.w5)((()=>[((0,s.wg)(!0),(0,s.iD)(s.HY,null,(0,s.Ko)(l.WIPData,(t=>((0,s.wg)(),(0,s.j4)(u,{key:t.WIPName,title:t.WIPName},{default:(0,s.w5)((()=>[(0,s._)("div",a,[(0,s.Wm)(c,{rack_info:t},null,8,["rack_info"])])])),_:2},1032,["title"])))),128))])),_:1})])}var n=r(3577);const l={class:"rack-status rounded m-3 p-1"},c={class:"my-1 d-flex flex-row bg-light"},u={class:"w-50 text-start px-2"},d={class:"p-1"},p={class:"flex-fill p-2"};function _(t,e,r,o,a,i){const _=(0,s.up)("el-progress"),f=(0,s.up)("RackPort");return(0,s.wg)(),(0,s.iD)("div",l,[(0,s._)("div",c,[(0,s._)("h3",u,(0,n.zw)(r.rack_info.WIPName),1),(0,s._)("div",d,[(0,s._)("b",null,(0,n.zw)(t.$t("Rack.Cargo_Spaces")),1)]),(0,s._)("div",p,[(0,s.Wm)(_,{"stroke-width":18,percentage:i.Level,"text-inside":"",striped:"","striped-flow":"",duration:40},{default:(0,s.w5)((()=>[(0,s._)("span",null,(0,n.zw)(this.HasCstPortNum)+"/"+(0,n.zw)(this.TotalPorts),1)])),_:1},8,["percentage"])])]),((0,s.wg)(!0),(0,s.iD)(s.HY,null,(0,s.Ko)(i.RowsArray(r.rack_info.Rows),(t=>((0,s.wg)(),(0,s.iD)("div",{class:"d-flex flex-row",key:"row-"+t},[((0,s.wg)(!0),(0,s.iD)(s.HY,null,(0,s.Ko)(r.rack_info.Columns,(e=>((0,s.wg)(),(0,s.iD)("div",{class:"d-flex flex-column",key:"col-"+e},[(0,s.Wm)(f,{rack_name:r.rack_info.WIPName,port_info:i.GetPortByColRow(e-1,t-1)},null,8,["rack_name","port_info"])])))),128))])))),128))])}r(57658);var f=r(49963);const y=t=>((0,s.dD)("data-v-2d637ab5"),t=t(),(0,s.Cn)(),t),m={class:"bg-light border-bottom d-flex py-1"},k={class:"text-danger bg-light w-100 text-start",style:{"max-height":"0",position:"relative",left:"3px",top:"0px"}},S=y((()=>(0,s._)("i",{class:"bi bi-exclamation"},null,-1))),x={class:"flex-fill text-start px-1"},C={class:"px-2"},v={class:"item"},g=y((()=>(0,s._)("div",{class:"title"},"Carrier ID",-1))),w={class:"values d-flex"},P={key:0,class:"item"},I=y((()=>(0,s._)("div",{class:"title"},"Exist Sensor(Tray)",-1))),D={class:"values d-flex"},h={key:1,class:"item"},R=y((()=>(0,s._)("div",{class:"title"},"Exist Sensor(Rack)",-1))),E={class:"values d-flex"},T={class:"item"},b=y((()=>(0,s._)("div",{class:"title"},"Install Priority",-1))),W={class:"values"},O={class:"item"},N=y((()=>(0,s._)("div",{class:"title"},"Install Time",-1))),A={class:"values"},$={class:"item justify-content-center"},j={key:0,class:"w-100 d-flex justify-content-center"};function H(t,e,r,o,a,i){const l=(0,s.up)("el-tag"),c=(0,s.up)("el-input"),u=(0,s.up)("el-tooltip"),d=(0,s.up)("el-button");return(0,s.wg)(),(0,s.iD)("div",{class:(0,n.C_)(["rack-port",i.ProductQualityClassName])},[(0,s._)("div",m,[(0,s.wy)((0,s._)("div",k,[S,(0,s.Uk)((0,n.zw)(t.$t("Rack.Sensor_Flash")),1)],512),[[f.F8,i.AnySensorFlash]]),(0,s._)("span",x,[(0,s.Wm)(l,{effect:"dark"},{default:(0,s.w5)((()=>[(0,s.Uk)((0,n.zw)(i.PortNameDisplay),1)])),_:1})]),(0,s._)("div",C,[0==r.port_info.Properties.ProductionQualityStore?((0,s.wg)(),(0,s.j4)(l,{key:0,class:(0,n.C_)(i.ProductQualityClassName+" text-dark"),effect:"dark"},{default:(0,s.w5)((()=>[(0,s.Uk)("NORMAL PORT")])),_:1},8,["class"])):((0,s.wg)(),(0,s.j4)(l,{key:1,class:(0,n.C_)(i.ProductQualityClassName+" text-dark"),effect:"dark"},{default:(0,s.w5)((()=>[(0,s.Uk)("NG PORT")])),_:1},8,["class"]))])]),(0,s._)("div",v,[g,(0,s._)("div",w,[(0,s.Wm)(c,{type:"text",disabled:"",size:"small",modelValue:r.port_info.CarrierID,"onUpdate:modelValue":e[0]||(e[0]=t=>r.port_info.CarrierID=t)},null,8,["modelValue"]),(0,s.Wm)(u,{placement:"top-start",content:t.$t("Rack.copy")},{default:(0,s.w5)((()=>[""!=r.port_info.CarrierID?((0,s.wg)(),(0,s.iD)("i",{key:0,onClick:e[1]||(e[1]=t=>i.CopyText(r.port_info.CarrierID)),class:"copy-button copy-icon bi bi-clipboard"})):(0,s.kq)("",!0)])),_:1},8,["content"])])]),2==r.port_info.Properties.CargoTypeStore||0==r.port_info.Properties.CargoTypeStore?((0,s.wg)(),(0,s.iD)("div",P,[I,(0,s._)("div",D,[(0,s._)("div",{class:"exist-sensor round my-1",style:(0,n.j5)(i.ExistSensorTray_1?a.ExistSensorOnStyle:a.ExistSensorOFFStyle),onClick:e[2]||(e[2]=t=>i.HandleExistSensorStateClick("tray",0))},null,4),(0,s._)("div",{class:"exist-sensor round my-1 mx-3",style:(0,n.j5)(i.ExistSensorTray_2?a.ExistSensorOnStyle:a.ExistSensorOFFStyle),onClick:e[3]||(e[3]=t=>i.HandleExistSensorStateClick("tray",1))},null,4)])])):(0,s.kq)("",!0),2==r.port_info.Properties.CargoTypeStore||1==r.port_info.Properties.CargoTypeStore?((0,s.wg)(),(0,s.iD)("div",h,[R,(0,s._)("div",E,[(0,s._)("div",{class:"exist-sensor round my-1",style:(0,n.j5)(i.ExistSensorRack_1?a.ExistSensorOnStyle:a.ExistSensorOFFStyle),onClick:e[4]||(e[4]=t=>i.HandleExistSensorStateClick("rack",0))},null,4),(0,s._)("div",{class:"exist-sensor round my-1 mx-3",style:(0,n.j5)(i.ExistSensorRack_2?a.ExistSensorOnStyle:a.ExistSensorOFFStyle),onClick:e[5]||(e[5]=t=>i.HandleExistSensorStateClick("rack",1))},null,4)])])):(0,s.kq)("",!0),(0,s._)("div",T,[b,(0,s._)("div",W,[(0,s.Wm)(c,{type:"text",size:"small",modelValue:r.port_info.Properties.StoragePriority,"onUpdate:modelValue":e[6]||(e[6]=t=>r.port_info.Properties.StoragePriority=t)},null,8,["modelValue"])])]),(0,s._)("div",O,[N,(0,s._)("div",A,(0,n.zw)(r.port_info.InstallTime),1)]),(0,s._)("div",$,[i.IsCarrierIDExist?((0,s.wg)(),(0,s.iD)("div",j,[(0,s.Wm)(d,{ref:"modify_btn",onClick:i.CstIDEditHandle,type:"success"},{default:(0,s.w5)((()=>[(0,s.Uk)((0,n.zw)(t.$t("Rack.Edit_ID")),1)])),_:1},8,["onClick"]),(0,s.Wm)(d,{onClick:i.RemoveCSTID,type:"danger"},{default:(0,s.w5)((()=>[(0,s.Uk)((0,n.zw)(t.$t("Rack.Remove_ID")),1)])),_:1},8,["onClick"])])):((0,s.wg)(),(0,s.j4)(d,{key:1,onClick:i.CstIDEditHandle,class:"m-1",type:"info"},{default:(0,s.w5)((()=>[(0,s.Uk)((0,n.zw)(t.$t("Rack.Creat_ID")),1)])),_:1},8,["onClick"]))])],2)}var F=r(42152),z=r.n(F),B=r(10844),U=r(49996),K=r(9669),L=r.n(K),Y=r(663),V=L().create({baseURL:Y.Z.backend_host});async function M(t,e,r){var s=await V.post(`/api/WIP/ModifyCargoID?WIPID=${t}&PortID=${e}&NewCargoID=${r}`);return s.data}async function Q(t,e){var r=await V.post(`/api/WIP/RemoveCargoID?WIPID=${t}&PortID=${e}`);return r.data}var Z=r(24239),q=r(64491),G={props:{rack_name:{type:String,default:""},port_info:{type:Object,default(){return{CargoExist:!1,CarrierID:null,ExistSensorStates:{TRAY_1:!1,TRAY_2:!0,RACK_1:!1,RACK_2:!1},InstallTime:"0001-01-01T00:00:00",Properties:{ID:"0-0",Row:0,Column:0,ProductionQualityStore:0,CargoTypeStore:2,IOLocation:{Tray_Sensor1:0,Tray_Sensor2:1,Box_Sensor1:2,Box_Sensor2:3},StoragePriority:0},RackPlacementState:0,TrayPlacementState:0}}}},data(){return{ExistSensorOnStyle:{backgroundColor:"lime"},ExistSensorOFFStyle:{backgroundColor:"rgb(255, 50, 0)"}}},computed:{ProductQualityClassName(){return 0==this.port_info.Properties.ProductionQualityStore?"ok-port":"ng-port"},PortNameDisplay(){return`${this.port_info.Properties.ID}`},IsCarrierIDExist(){return this.port_info.CarrierID&&""!=this.port_info.CarrierID},ModifyButtonText(){return this.IsCarrierIDExist?"修改帳籍":"新增帳籍"},ExistSensorTray_1(){return 0!=this.port_info.ExistSensorStates["TRAY_1"]},ExistSensorTray_2(){return 0!=this.port_info.ExistSensorStates["TRAY_2"]},ExistSensorRack_1(){return 0!=this.port_info.ExistSensorStates["RACK_1"]},ExistSensorRack_2(){return 0!=this.port_info.ExistSensorStates["RACK_2"]},AnySensorFlash(){var t=Object.values(this.port_info.ExistSensorStates),e=t.filter((t=>2==t));return 0!=e.length},IsDeveloperLogining(){return Z.HP.getters.IsDeveloperLogining}},methods:{async HandleExistSensorStateClick(t="tray|rack",e=1){if(this.IsDeveloperLogining){var r=await q.k3.SetSensorState(this.rack_name,this.port_info.Properties.ID,t,e,!1);r.confirm||this.$swal.fire({text:"",title:`${r.message}`,icon:"info",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"})}},CstIDEditHandle(){B.T.prompt("Carrier ID",{title:`${this.ModifyButtonText} : ${this.PortNameDisplay}`,draggable:!0,inputValue:this.port_info.CarrierID,type:"warning",confirmButtonText:"修改",inputErrorMessage:"帳籍ID不得為空",inputPlaceholder:"請輸入ID",inputValidator:t=>""!=t}).then((t=>{console.info(t);var e=t.value?t.value:"";M(this.rack_name,this.port_info.Properties.ID,e)})).catch((()=>{}))},RemoveCSTID(){Q(this.rack_name,this.port_info.Properties.ID)},CopyText(t){const e=new(z())(".copy-button",{text:()=>t});e.on("success",(()=>{(0,U.bM)({title:t,message:"已複製到剪貼簿",duration:1500}),e.destroy()})),e.on("error",(()=>{e.destroy()}))}}},J=r(83744);const X=(0,J.Z)(G,[["render",H],["__scopeId","data-v-2d637ab5"]]);var tt=X,et={components:{RackPort:tt},methods:{GetPortByColRow(t,e){var r=this.rack_info.Ports;return r.find((r=>r.Properties.Row==e&&r.Properties.Column==t))},RowsArray(t){let e=[];for(var r=t;r>=1;r--)e.push(r);return e}},props:{rack_info:{type:Object,default(){return{WIPName:"Rack-1",Rows:3,Columns:3,ColumnsTagMap:{0:[0],1:[1],2:[2]},Ports:[{CargoExist:!1,CarrierID:null,ExistSensorStates:{TRAY_1:!1,TRAY_2:!0,RACK_1:!1,RACK_2:!1},InstallTime:"0001-01-01T00:00:00",Properties:{ID:"0-0",Row:0,Column:0,IOLocation:{Tray_Sensor1:0,Tray_Sensor2:1,Box_Sensor1:2,Box_Sensor2:3}},RackPlacementState:0,TrayPlacementState:0}]}}}},computed:{TotalPorts(){return this.rack_info.Rows*this.rack_info.Columns},HasCstPortNum(){let t=0;for(let e=0;e<this.rack_info.Ports.length;e++)!0===this.rack_info.Ports[e].CargoExist&&t++;return t},Level(){return this.HasCstPortNum/this.TotalPorts*100}}};const rt=(0,J.Z)(et,[["render",_],["__scopeId","data-v-b26db9a6"]]);var st=rt,ot={components:{RackStatus:st},data(){return{}},computed:{WIPData(){return Z.jU.getters.WIPData}}};const at=(0,J.Z)(ot,[["render",i]]);var it=at}}]);
//# sourceMappingURL=15.c65c3e09.js.map