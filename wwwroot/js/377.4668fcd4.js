"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[377],{98377:function(e,l,a){a.r(l),a.d(l,{default:function(){return y}});var t=a(66252),o=a(3577);const n=e=>((0,t.dD)("data-v-0c2a8de8"),e=e(),(0,t.Cn)(),e),d={class:"equipment-manager border-start border-end"},i={class:"text-start px-1"},s={class:""},_={class:"w-100 d-flex flex-row"},m={class:"w-100"},u={class:"d-flex w-100 mx-1"},r={class:"w-50 border p-2"},c=n((()=>(0,t._)("h5",{class:"w-100 border rounded bg-primary text-light"},"INPUT",-1))),p={class:"w-50 border p-2 mx-1"},w=n((()=>(0,t._)("h5",{class:"w-100 border rounded bg-info text-light"},"OUTPUT",-1))),V={class:"hs-signals d-flex"},g=n((()=>(0,t._)("div",{class:"mx-3"},"交握訊號-EQ",-1))),h={class:"hs-signals d-flex"},q=n((()=>(0,t._)("div",{class:"mx-3"},"交握訊號-AGV",-1)));function b(e,l,a,n,b,E){const f=(0,t.up)("b-button"),C=(0,t.up)("el-table-column"),W=(0,t.up)("b-form-input"),O=(0,t.up)("el-button"),U=(0,t.up)("el-option"),D=(0,t.up)("el-select"),v=(0,t.up)("el-checkbox"),S=(0,t.up)("el-input"),y=(0,t.up)("el-table"),T=(0,t.up)("el-divider"),H=(0,t.up)("el-form-item"),I=(0,t.up)("el-form"),N=(0,t.up)("el-drawer"),P=(0,t.Q2)("loading");return(0,t.wy)(((0,t.wg)(),(0,t.iD)("div",d,[(0,t._)("p",i,[(0,t.Wm)(f,{variant:"primary",squared:"",onClick:E.SaveSettingHandler},{default:(0,t.w5)((()=>[(0,t.Uk)("儲存設定")])),_:1},8,["onClick"]),(0,t.Wm)(f,{variant:"info",squared:"",class:"mx-2",onClick:E.AddNewEqHandler},{default:(0,t.w5)((()=>[(0,t.Uk)("新增設備")])),_:1},8,["onClick"]),(0,t.Wm)(f,{squared:"",onClick:E.ReloadSettingsHandler},{default:(0,t.w5)((()=>[(0,t.Uk)("重新載入")])),_:1},8,["onClick"])]),(0,t.Wm)(y,{"header-cell-style":{color:"white",backgroundColor:"rgb(13, 110, 253)",fontSize:"12px"},data:b.EqDatas,"row-key":b.rowKey,size:"small",border:"","scrollbar-always-on":"",height:"77vh","table-layout":"fixed",ref:"eqTable",style:{width:"100vw"}},{default:(0,t.w5)((()=>[(0,t.Wm)(C,{label:"Index",prop:"index",width:"80",align:"center",fixed:"left"}),(0,t.Wm)(C,{label:"設備名稱",prop:"Name",width:"210",fixed:"left"},{default:(0,t.w5)((e=>[(0,t._)("div",s,[(0,t.Wm)(W,{state:b.ValidName(e.row),modelValue:e.row.Name,"onUpdate:modelValue":l=>e.row.Name=l,placeholder:"設備名稱",style:{width:"160px"},"no-wheel":!0,size:"sm",min:1,onInput:l=>E.HandleEqNameChange(e.row,e.row.Name)},null,8,["state","modelValue","onUpdate:modelValue","onInput"]),(0,t.Wm)(O,{class:"my-1",size:"small",onClick:l=>E.HandleUseMapDataDisplayName(e.row.TagID)},{default:(0,t.w5)((()=>[(0,t.Uk)("使用圖資設定")])),_:2},1032,["onClick"])])])),_:1}),(0,t.Wm)(C,{label:"層",prop:"Height",width:"80",align:"center",fixed:"left"},{default:(0,t.w5)((e=>[(0,t.Wm)(D,{modelValue:e.row.Height,"onUpdate:modelValue":l=>e.row.Height=l},{default:(0,t.w5)((()=>[(0,t.Wm)(U,{label:"1",value:0}),(0,t.Wm)(U,{label:"2",value:1})])),_:2},1032,["modelValue","onUpdate:modelValue"])])),_:1}),(0,t.Wm)(C,{label:"Tag ID",prop:"TagID",width:"120",align:"center",fixed:"left"},{default:(0,t.w5)((e=>[(0,t.Wm)(W,{type:"number",state:b.ValidTag(e.row),modelValue:e.row.TagID,"onUpdate:modelValue":l=>e.row.TagID=l,modelModifiers:{number:!0},placeholder:"tag id",size:"sm",min:1},null,8,["state","modelValue","onUpdate:modelValue"])])),_:1}),(0,t.Wm)(C,{label:"下游設備",width:"850"},{default:(0,t.w5)((e=>[(0,t._)("div",_,[(0,t.Wm)(D,{size:"small",modelValue:e.row.ValidDownStreamEndPointNames,"onUpdate:modelValue":l=>e.row.ValidDownStreamEndPointNames=l,multiple:"",placeholder:"Select",style:{width:"1000px"}},{default:(0,t.w5)((()=>[((0,t.wg)(!0),(0,t.iD)(t.HY,null,(0,t.Ko)(E.GetAvaluableEqNameList(e.row.Name),(e=>((0,t.wg)(),(0,t.j4)(U,{key:e,label:e,value:e},null,8,["label","value"])))),128))])),_:2},1032,["modelValue","onUpdate:modelValue"]),(0,t.Wm)(O,{class:"mx-1",size:"small",type:"default",onClick:()=>{e.row.ValidDownStreamEndPointNames=E.GetAvaluableEqNameList(e.row.Name)}},{default:(0,t.w5)((()=>[(0,t.Uk)("使用所有設備")])),_:2},1032,["onClick"]),(0,t.Wm)(O,{size:"small",type:"danger",onClick:()=>{e.row.ValidDownStreamEndPointNames=[]}},{default:(0,t.w5)((()=>[(0,t.Uk)("清除")])),_:2},1032,["onClick"])])])),_:1}),(0,t.Wm)(C,{label:"允入車款",width:"120"},{default:(0,t.w5)((e=>[(0,t.Wm)(D,{size:"small",modelValue:e.row.Accept_AGV_Type,"onUpdate:modelValue":l=>e.row.Accept_AGV_Type=l},{default:(0,t.w5)((()=>[(0,t.Wm)(U,{value:0,label:"0-不限"}),(0,t.Wm)(U,{value:1,label:"1-叉車AGV"}),(0,t.Wm)(U,{value:2,label:"2-潛盾AGV"})])),_:2},1032,["modelValue","onUpdate:modelValue"])])),_:1}),(0,t.Wm)(C,{label:"可移載貨物類型",width:"130"},{default:(0,t.w5)((e=>[(0,t.Wm)(D,{size:"small",modelValue:e.row.EQAcceeptCargoType,"onUpdate:modelValue":l=>e.row.EQAcceeptCargoType=l},{default:(0,t.w5)((()=>[(0,t.Wm)(U,{value:0,label:"0-不限"}),(0,t.Wm)(U,{value:200,label:"200-Tray"}),(0,t.Wm)(U,{value:201,label:"201-子母框"})])),_:2},1032,["modelValue","onUpdate:modelValue"])])),_:1}),(0,t.Wm)(C,{label:"具貨物升降機構",width:"100",align:"center"},{default:(0,t.w5)((e=>[(0,t.Wm)(v,{modelValue:e.row.HasCstSteeringMechanism,"onUpdate:modelValue":l=>e.row.HasCstSteeringMechanism=l},null,8,["modelValue","onUpdate:modelValue"])])),_:1}),(0,t.Wm)(C,{label:"雙Port設備",width:"100",align:"center"},{default:(0,t.w5)((e=>[(0,t.Wm)(v,{modelValue:e.row.IsOneOfDualPorts,"onUpdate:modelValue":l=>e.row.IsOneOfDualPorts=l},null,8,["modelValue","onUpdate:modelValue"])])),_:1}),(0,t.Wm)(C,{label:"AnotherPortTagNumber",width:"80",align:"center"},{default:(0,t.w5)((e=>[(0,t.Wm)(S,{type:"number",modelValue:e.row.AnotherPortTagNumber,"onUpdate:modelValue":l=>e.row.AnotherPortTagNumber=l,disabled:!e.row.IsOneOfDualPorts},null,8,["modelValue","onUpdate:modelValue","disabled"])])),_:1}),(0,t.Wm)(C,{label:"可取貨狀態識別碼",width:"80",align:"center"},{default:(0,t.w5)((e=>[(0,t.Wm)(S,{type:"number",modelValue:e.row.AllowUnloadPortTypeNumber,"onUpdate:modelValue":l=>e.row.AllowUnloadPortTypeNumber=l,disabled:!e.row.IsOneOfDualPorts},null,8,["modelValue","onUpdate:modelValue","disabled"])])),_:1}),(0,t.Wm)(C,{label:"空框/實框 訊號檢查",width:"100",align:"center"},{default:(0,t.w5)((e=>[(0,t.Wm)(v,{modelValue:e.row.RackCapcityCheck,"onUpdate:modelValue":l=>e.row.RackCapcityCheck=l},null,8,["modelValue","onUpdate:modelValue"])])),_:1}),(0,t.Wm)(C,{label:"操作","min-width":"100",fixed:"right"},{default:(0,t.w5)((e=>[(0,t._)("div",null,[(0,t.Wm)(O,{size:"small",type:"default",onClick:l=>E.ConnectionSettingBtnHandle(e.row)},{default:(0,t.w5)((()=>[(0,t.Uk)("連線設定")])),_:2},1032,["onClick"]),(0,t.Wm)(O,{size:"small",type:"danger",onClick:l=>E.RemoveHandle(e.row)},{default:(0,t.w5)((()=>[(0,t.Uk)("移除")])),_:2},1032,["onClick"])])])),_:1})])),_:1},8,["header-cell-style","data","row-key"]),(0,t.Wm)(N,{size:"50%","z-index":1,modelValue:b.connection_setting_drawer,"onUpdate:modelValue":l[21]||(l[21]=e=>b.connection_setting_drawer=e),title:`${b.selected_eq_option.Name}-連線設定`},{default:(0,t.w5)((()=>[(0,t._)("div",m,[(0,t.Wm)(I,{"label-position":"left","label-width":"100"},{default:(0,t.w5)((()=>[(0,t.Wm)(T,null,{default:(0,t.w5)((()=>[(0,t.Uk)("通訊 Protocol")])),_:1}),(0,t.Wm)(H,{label:"通訊方式"},{default:(0,t.w5)((()=>[(0,t.Wm)(D,{modelValue:b.selected_eq_option.ConnOptions.ConnMethod,"onUpdate:modelValue":l[0]||(l[0]=e=>b.selected_eq_option.ConnOptions.ConnMethod=e)},{default:(0,t.w5)((()=>[(0,t.Wm)(U,{label:"Modbus TCP",value:0}),(0,t.Wm)(U,{label:"Modbus RTU",value:1}),(0,t.Wm)(U,{label:"TCP/IP",value:2}),(0,t.Wm)(U,{label:"Serial Port",value:3}),(0,t.Wm)(U,{label:"MC Protocol",value:5})])),_:1},8,["modelValue"])])),_:1}),(0,t.Wm)(H,{label:"IP"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{disabled:1==b.selected_eq_option.ConnOptions.ConnMethod,modelValue:b.selected_eq_option.ConnOptions.IP,"onUpdate:modelValue":l[1]||(l[1]=e=>b.selected_eq_option.ConnOptions.IP=e)},null,8,["disabled","modelValue"])])),_:1}),(0,t.Wm)(H,{label:"PORT"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{disabled:1==b.selected_eq_option.ConnOptions.ConnMethod,modelValue:b.selected_eq_option.ConnOptions.Port,"onUpdate:modelValue":l[2]||(l[2]=e=>b.selected_eq_option.ConnOptions.Port=e),modelModifiers:{number:!0}},null,8,["disabled","modelValue"])])),_:1}),(0,t.Wm)(H,{label:"COMPORT"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{disabled:0==b.selected_eq_option.ConnOptions.ConnMethod,modelValue:b.selected_eq_option.ConnOptions.ComPort,"onUpdate:modelValue":l[3]||(l[3]=e=>b.selected_eq_option.ConnOptions.ComPort=e)},null,8,["disabled","modelValue"])])),_:1}),(0,t.Wm)(O,{loading:b.connection_testing,type:"default",onClick:l[4]||(l[4]=e=>E.ConnectTestHandle(b.selected_eq_option))},{default:(0,t.w5)((()=>[(0,t.Uk)("通訊測試")])),_:1},8,["loading"]),(0,t.Wm)(T,null,{default:(0,t.w5)((()=>[(0,t.Uk)("IO位置")])),_:1}),(0,t.Wm)(H,{label:"IO數量"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{type:"number",modelValue:b.selected_eq_option.ConnOptions.Input_RegisterNum,"onUpdate:modelValue":l[5]||(l[5]=e=>b.selected_eq_option.ConnOptions.Input_RegisterNum=e)},null,8,["modelValue"])])),_:1}),(0,t.Wm)(H,{label:"PLC Base"},{default:(0,t.w5)((()=>[(0,t.Wm)(v,{modelValue:b.selected_eq_option.ConnOptions.IsPLCAddress_Base_1,"onUpdate:modelValue":l[6]||(l[6]=e=>b.selected_eq_option.ConnOptions.IsPLCAddress_Base_1=e)},null,8,["modelValue"])])),_:1}),(0,t._)("div",u,[(0,t._)("div",r,[c,(0,t.Wm)(I,{"label-position":"left","label-width":"120"},{default:(0,t.w5)((()=>[(0,t.Wm)(H,{label:"Start Index"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{type:"number",modelValue:b.selected_eq_option.ConnOptions.Input_StartRegister,"onUpdate:modelValue":l[7]||(l[7]=e=>b.selected_eq_option.ConnOptions.Input_StartRegister=e)},null,8,["modelValue"])])),_:1}),(0,t.Wm)(T),(0,t.Wm)(H,{label:"Load_Request"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{type:"number",modelValue:b.selected_eq_option.IOLocation.Load_Request,"onUpdate:modelValue":l[8]||(l[8]=e=>b.selected_eq_option.IOLocation.Load_Request=e)},null,8,["modelValue"])])),_:1}),(0,t.Wm)(H,{label:"Unload_Request"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{type:"number",modelValue:b.selected_eq_option.IOLocation.Unload_Request,"onUpdate:modelValue":l[9]||(l[9]=e=>b.selected_eq_option.IOLocation.Unload_Request=e)},null,8,["modelValue"])])),_:1}),(0,t.Wm)(H,{label:"Port_Exist"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{type:"number",modelValue:b.selected_eq_option.IOLocation.Port_Exist,"onUpdate:modelValue":l[10]||(l[10]=e=>b.selected_eq_option.IOLocation.Port_Exist=e)},null,8,["modelValue"])])),_:1}),(0,t.Wm)(H,{label:"Up_Pose"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{type:"number",modelValue:b.selected_eq_option.IOLocation.Up_Pose,"onUpdate:modelValue":l[11]||(l[11]=e=>b.selected_eq_option.IOLocation.Up_Pose=e)},null,8,["modelValue"])])),_:1}),(0,t.Wm)(H,{label:"Down_Pose"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{type:"number",modelValue:b.selected_eq_option.IOLocation.Down_Pose,"onUpdate:modelValue":l[12]||(l[12]=e=>b.selected_eq_option.IOLocation.Down_Pose=e)},null,8,["modelValue"])])),_:1}),(0,t.Wm)(H,{label:"Eqp_Status_Down"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{type:"number",modelValue:b.selected_eq_option.IOLocation.Eqp_Status_Down,"onUpdate:modelValue":l[13]||(l[13]=e=>b.selected_eq_option.IOLocation.Eqp_Status_Down=e)},null,8,["modelValue"])])),_:1}),(0,t.Wm)(H,{label:"Eqp_Maintaining"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{type:"number",modelValue:b.selected_eq_option.IOLocation.Eqp_Maintaining,"onUpdate:modelValue":l[14]||(l[14]=e=>b.selected_eq_option.IOLocation.Eqp_Maintaining=e)},null,8,["modelValue"])])),_:1}),(0,t.Wm)(H,{label:"Eqp_PartsReplacing"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{type:"number",modelValue:b.selected_eq_option.IOLocation.Eqp_PartsReplacing,"onUpdate:modelValue":l[15]||(l[15]=e=>b.selected_eq_option.IOLocation.Eqp_PartsReplacing=e)},null,8,["modelValue"])])),_:1})])),_:1})]),(0,t._)("div",p,[w,(0,t.Wm)(I,{"label-position":"left","label-width":"120"},{default:(0,t.w5)((()=>[(0,t.Wm)(H,{label:"Start Index"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{type:"number",modelValue:b.selected_eq_option.ConnOptions.Output_Start_Address,"onUpdate:modelValue":l[16]||(l[16]=e=>b.selected_eq_option.ConnOptions.Output_Start_Address=e)},null,8,["modelValue"])])),_:1}),(0,t.Wm)(T),(0,t.Wm)(H,{label:"To_EQ_Up"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{type:"number",modelValue:b.selected_eq_option.IOLocation.To_EQ_Up,"onUpdate:modelValue":l[17]||(l[17]=e=>b.selected_eq_option.IOLocation.To_EQ_Up=e)},null,8,["modelValue"])])),_:1}),(0,t.Wm)(H,{label:"To_EQ_Low"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{type:"number",modelValue:b.selected_eq_option.IOLocation.To_EQ_Low,"onUpdate:modelValue":l[18]||(l[18]=e=>b.selected_eq_option.IOLocation.To_EQ_Low=e)},null,8,["modelValue"])])),_:1}),(0,t.Wm)(H,{label:"CMD_Reserve_Up"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{type:"number",modelValue:b.selected_eq_option.IOLocation.CMD_Reserve_Up,"onUpdate:modelValue":l[19]||(l[19]=e=>b.selected_eq_option.IOLocation.CMD_Reserve_Up=e)},null,8,["modelValue"])])),_:1}),(0,t.Wm)(H,{label:"CMD_Reserve_Low"},{default:(0,t.w5)((()=>[(0,t.Wm)(S,{type:"number",modelValue:b.selected_eq_option.IOLocation.CMD_Reserve_Low,"onUpdate:modelValue":l[20]||(l[20]=e=>b.selected_eq_option.IOLocation.CMD_Reserve_Low=e)},null,8,["modelValue"])])),_:1})])),_:1})])])])),_:1})])])),_:1},8,["modelValue","title"]),(0,t.Wm)(N,{modelValue:b.io_check_drawer,"onUpdate:modelValue":l[35]||(l[35]=e=>b.io_check_drawer=e),direction:"btt"},{default:(0,t.w5)((()=>[(0,t._)("div",V,[g,(0,t._)("div",{class:"di-status",onClick:l[22]||(l[22]=l=>e.HandleHSsignaleChange(E.selected_eq_io_data.EQName,"L_REQ",!e.scope.row.HS_EQ_L_REQ)),style:(0,o.j5)(e.signalOn(E.selected_eq_io_data.HS_EQ_L_REQ))},"L_REQ",4),(0,t._)("div",{class:"di-status",onClick:l[23]||(l[23]=l=>e.HandleHSsignaleChange(E.selected_eq_io_data.EQName,"U_REQ",!e.scope.row.HS_EQ_U_REQ)),style:(0,o.j5)(e.signalOn(E.selected_eq_io_data.HS_EQ_U_REQ))},"U_REQ",4),(0,t._)("div",{class:"di-status",onClick:l[24]||(l[24]=l=>e.HandleHSsignaleChange(E.selected_eq_io_data.EQName,"READY",!e.scope.row.HS_EQ_READY)),style:(0,o.j5)(e.signalOn(E.selected_eq_io_data.HS_EQ_READY))},"READY",4),(0,t._)("div",{class:"di-status",onClick:l[25]||(l[25]=l=>e.HandleHSsignaleChange(E.selected_eq_io_data.EQName,"UP_READY",!e.scope.row.HS_EQ_UP_READY)),style:(0,o.j5)(e.signalOn(E.selected_eq_io_data.HS_EQ_UP_READY))},"UP_READY",4),(0,t._)("div",{class:"di-status",onClick:l[26]||(l[26]=l=>e.HandleHSsignaleChange(E.selected_eq_io_data.EQName,"LOW_READY",!e.scope.row.HS_EQ_LOW_READY)),style:(0,o.j5)(e.signalOn(E.selected_eq_io_data.HS_EQ_LOW_READY))},"LOW_READY",4),(0,t.kq)("",!0)]),(0,t._)("div",h,[q,(0,t._)("div",{class:"di-status",onClick:l[28]||(l[28]=l=>e.HandleAGVHSSignaleChange(E.selected_eq_io_data.EQName,"To_EQ_Up",!e.scope.row.To_EQ_Up)),style:(0,o.j5)(e.signalOn(E.selected_eq_io_data.To_EQ_Up))},"To_EQ_Up",4),(0,t._)("div",{class:"di-status",onClick:l[29]||(l[29]=l=>e.HandleAGVHSSignaleChange(E.selected_eq_io_data.EQName,"To_EQ_Low",!e.scope.row.To_EQ_Low)),style:(0,o.j5)(e.signalOn(E.selected_eq_io_data.To_EQ_Low))},"To_EQ_Low",4),(0,t._)("div",{class:"di-status",onClick:l[30]||(l[30]=l=>e.HandleAGVHSSignaleChange(E.selected_eq_io_data.EQName,"VALID",!e.scope.row.HS_AGV_VALID)),style:(0,o.j5)(e.signalOn(E.selected_eq_io_data.HS_AGV_VALID))},"VALID",4),(0,t._)("div",{class:"di-status",onClick:l[31]||(l[31]=l=>e.HandleAGVHSSignaleChange(E.selected_eq_io_data.EQName,"TR_REQ",!e.scope.row.HS_AGV_TR_REQ)),style:(0,o.j5)(e.signalOn(E.selected_eq_io_data.HS_AGV_TR_REQ))},"TR_REQ",4),(0,t._)("div",{class:"di-status",onClick:l[32]||(l[32]=l=>e.HandleAGVHSSignaleChange(E.selected_eq_io_data.EQName,"BUSY",!e.scope.row.HS_AGV_BUSY)),style:(0,o.j5)(e.signalOn(E.selected_eq_io_data.HS_AGV_BUSY))},"BUSY",4),(0,t.kq)("",!0),(0,t._)("div",{class:"di-status",onClick:l[34]||(l[34]=l=>e.HandleAGVHSSignaleChange(E.selected_eq_io_data.EQName,"COMPT",!e.scope.row.HS_AGV_COMPT)),style:(0,o.j5)(e.signalOn(E.selected_eq_io_data.HS_AGV_COMPT))},"COMPT",4)])])),_:1},8,["modelValue"])])),[[P,b.loading]])}a(57658);var E=a(64491),f=a(91747),C=a(95320),W=a(24239),O=a(49996),U=(a(30381),a(78538)),D={components:{RegionsSelector:f.Z},data(){return{cell_item_size:"",io_check_drawer:!1,connection_setting_drawer:!1,EqDatas:[],EqDatas_Orignal:[],ValidTag:e=>!0,ValidName:e=>{var l=e.Name,a=this.EqDatas.filter((l=>l.Name==e.Name));return 1==a.length&&""!=l},selected_eq_option:new U.W,connection_testing:!1,loading:!0,rowKey:"index"}},methods:{GetAvaluableEqNameList(e){var l=this.EqNames.filter((l=>l!=e));return["ALL",...l]},async SaveSettingHandler(){var e=this.ValidateSetting();if(e.confirm){var l=await(0,E.wW)(this.EqDatas);l.confirm?(setTimeout((()=>{this.ReloadSettingsHandler(!1)}),200),this.$swal.fire({title:"儲存成功",icon:"success",timer:2e3})):this.$swal.fire({title:"參數設定有誤",text:l.message,icon:"error"})}else this.$swal.fire({text:"",title:e.message,icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"})},ValidateSetting(){const e=this.EqDatas.length;var l=[...new Set(this.EqDatas.map((e=>e.TagID+"-"+e.Height)))];const a=l.length==e;return{confirm:a,message:"同一個TAG不可有相同的層設置"}},async DownloadEQOptions(){this.EqDatas=[];var e=W.jU.state.EqOptions;for(let a=0;a<e.length;a++){var l=new U.W;Object.assign(l,e[a]),l.index=a,this.EqDatas.push(l)}this.CloneEQDatas()},CloneEQDatas(){this.EqDatas_Orignal=JSON.parse(JSON.stringify(this.EqDatas))},ReloadSettingsHandler(e=!0){this.loading=e,setTimeout((()=>{(0,E.H5)().then((e=>W.jU.commit("EqOptions",e))),this.DownloadEQOptions(),this.$refs.eqTable.setScrollTop(0),this.loading=!1}),300)},AddNewEqHandler(){let e=new U.W;e.TagID=1,e.index=this.EqDatas.length,this.EqDatas=[e,...this.EqDatas],setTimeout((()=>{this.$refs.eqTable.setScrollTop(0)}),300)},RemoveHandle(e){var l=this.EqDatas.filter((l=>l.Name!=e.Name));this.EqDatas=l},async IOCheckBtnHandle(e){this.selected_eq_option=e,this.io_check_drawer=!0},async ConnectionSettingBtnHandle(e){this.selected_eq_option=e,this.connection_setting_drawer=!0},async ConnectTestHandle(e){this.connection_testing=!0;var l=await(0,E.qN)(e.ConnOptions);this.connection_testing=!1;var a="";a=0==e.ConnOptions.ConnMethod?`Modbus TCP - ${e.ConnOptions.IP}:${e.ConnOptions.Port}`:`Modbus RTU - ${e.ConnOptions.ComPort}`,l.Connected?this.$swal.fire({title:"OK",text:a,icon:"success"}):this.$swal.fire({title:"Fail",text:a,icon:"error"})},beforeRouteLeave(e,l,a){alert("leave!")},async HandleUseMapDataDisplayName(e){var l=await C.p.dispatch("GetMapPointByTag",e);if(l){var a=this.EqDatas.find((l=>l.TagID==e)),t=l.Graph.Display;a.Name=t,this.HandleEqNameChange(a,t),(0,O.bM)({message:`Get Display Name From Map Success(Tag ${e} = ${t})`,duration:1e3,type:"success",title:"設備同步名稱"})}else(0,O.bM)({message:"Get Display Name From Map Fail",duration:1e3,type:"error",title:"設備同步名稱失敗"})},HandleEqNameChange(e,l){var a=e.TagID,t=this.EqDatas_Orignal.find((e=>e.TagID==a));if(t){var o=t.Name,n=this.EqDatas.filter((e=>e.ValidDownStreamEndPointNames.includes(o)));n.forEach((e=>{var a=e.ValidDownStreamEndPointNames.indexOf(o);e.ValidDownStreamEndPointNames[a]=l})),this.CloneEQDatas()}}},mounted(){setTimeout((()=>{this.DownloadEQOptions(),this.loading=!1}),400)},computed:{EqNames(){return this.EqDatas.map((e=>e.Name))},eq_data(){return W.jU.getters.EQData},selected_eq_io_data(){return this.eq_data.find((e=>e.EQName==this.selected_eq_option.EQName))}}},v=a(83744);const S=(0,v.Z)(D,[["render",b],["__scopeId","data-v-0c2a8de8"]]);var y=S}}]);
//# sourceMappingURL=377.4668fcd4.js.map