"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[485],{6485:function(e,t,a){a.r(t),a.d(t,{default:function(){return M}});var o=a(79003);const l={class:"agv_show_on_map_design h-100"};function i(e,t,a,i,r,n){const s=(0,o.resolveComponent)("AGVDrawSetting"),c=(0,o.resolveComponent)("el-main"),d=(0,o.resolveComponent)("el-container");return(0,o.openBlock)(),(0,o.createElementBlock)("div",l,[(0,o.createVNode)(d,{class:"h-100"},{default:(0,o.withCtx)((()=>[(0,o.createVNode)(c,{class:"bg-light p-2"},{default:(0,o.withCtx)((()=>[((0,o.openBlock)(!0),(0,o.createElementBlock)(o.Fragment,null,(0,o.renderList)(n.agvNameList,(e=>((0,o.openBlock)(),(0,o.createBlock)(s,{key:e,agv_name:e},null,8,["agv_name"])))),128))])),_:1})])),_:1})])}const r=e=>((0,o.pushScopeId)("data-v-443f0fb2"),e=e(),(0,o.popScopeId)(),e),n={class:"agv_draw_setting"},s={class:"agv-id w-100 border-bottom mb-1 text-start text-primary"},c={class:"w-100 d-flex flex-row"},d={class:""},p=r((()=>(0,o.createElementVNode)("div",{class:"text-start text-primary mb-3"},"Preview",-1))),m=["id"];function g(e,t,a,l,i,r){const g=(0,o.resolveComponent)("el-input"),u=(0,o.resolveComponent)("el-form-item"),h=(0,o.resolveComponent)("el-color-picker"),v=(0,o.resolveComponent)("ImageUploadButtonVue"),w=(0,o.resolveComponent)("el-form"),f=(0,o.resolveComponent)("el-checkbox"),C=(0,o.resolveComponent)("el-aside"),y=(0,o.resolveComponent)("el-main"),_=(0,o.resolveComponent)("el-container");return(0,o.openBlock)(),(0,o.createElementBlock)("div",n,[(0,o.createVNode)(_,{class:"h-100"},{default:(0,o.withCtx)((()=>[(0,o.createVNode)(C,{class:"properties-setting-container border bg-light",width:"50%"},{default:(0,o.withCtx)((()=>[(0,o.createElementVNode)("div",s,(0,o.toDisplayString)(a.agv_name),1),(0,o.createElementVNode)("div",c,[(0,o.createVNode)(w,{"label-width":"80","label-position":"left"},{default:(0,o.withCtx)((()=>[(0,o.createVNode)(u,{label:"顯示名稱"},{default:(0,o.withCtx)((()=>[(0,o.createVNode)(g,{type:"text",size:"small",modelValue:i.props.DisplayText,"onUpdate:modelValue":t[0]||(t[0]=e=>i.props.DisplayText=e),onInput:t[1]||(t[1]=e=>{r.ChangeTextOfAGV(e)})},null,8,["modelValue"])])),_:1}),(0,o.createVNode)(u,{label:"顏色"},{default:(0,o.withCtx)((()=>[(0,o.createVNode)(h,{modelValue:i.props.DisplayColor,"onUpdate:modelValue":t[2]||(t[2]=e=>i.props.DisplayColor=e),onActiveChange:r.HandleAGVColorClicked},null,8,["modelValue","onActiveChange"])])),_:1}),(0,o.createVNode)(u,{label:"ICON"},{default:(0,o.withCtx)((()=>[(0,o.createElementVNode)("div",d,[(0,o.createVNode)(v,{backendUrl:r.iconUploadAPIEndPoint,onOnFileUploaded:r.HandleImageUploaded},null,8,["backendUrl","onOnFileUploaded"])])])),_:1})])),_:1}),(0,o.createVNode)(w,{class:"mx-3","label-width":"120","label-position":"left"},{default:(0,o.withCtx)((()=>[(0,o.createVNode)(u,{label:"顯示貨物狀態"},{default:(0,o.withCtx)((()=>[(0,o.createVNode)(f)])),_:1}),(0,o.createVNode)(u,{label:"貨物-[框]圖示"}),(0,o.createVNode)(u,{label:"貨物-[Tray]圖示"})])),_:1})])])),_:1}),(0,o.createVNode)(y,{class:"border p-1",style:{"overflow-y":"hidden"}},{default:(0,o.withCtx)((()=>[p,(0,o.createElementVNode)("div",{id:r.map_id,class:"agv-preview-map border w-100",style:{height:"200px"}},null,8,m)])),_:1})])),_:1})])}var u=a(7686),h=a(88818),v=a(50563),w=a(98843),f=a(93292),C=a(1508),y=a(92949),_=a(79072),V=a(54404),x=a(93166),S=a(22205),F=a(7712),k=a(19966),b=a(95320),N=a(24239),B=a(53259);const U={class:"image-upload-button"},T={key:1,class:"image-preview"},Z=["src"];function A(e,t,a,l,i,r){const n=(0,o.resolveComponent)("el-button");return(0,o.openBlock)(),(0,o.createElementBlock)("div",U,[(0,o.createVNode)(n,{size:"small",type:"button",onClick:r.triggerFileInput},{default:(0,o.withCtx)((()=>[(0,o.createTextVNode)((0,o.toDisplayString)(a.buttonText),1)])),_:1},8,["onClick"]),null!=i.imageFile?((0,o.openBlock)(),(0,o.createBlock)(n,{key:0,size:"small",type:"button",onClick:r.uploadToServer},{default:(0,o.withCtx)((()=>[(0,o.createTextVNode)("確認上傳")])),_:1},8,["onClick"])):(0,o.createCommentVNode)("",!0),(0,o.createElementVNode)("input",{type:"file",ref:"fileInput",onChange:t[0]||(t[0]=(...e)=>r.handleFileChange&&r.handleFileChange(...e)),accept:"image/*",style:{display:"none"}},null,544),i.imageUrl&&a.showPreview?((0,o.openBlock)(),(0,o.createElementBlock)("div",T,[(0,o.createElementVNode)("img",{src:i.imageUrl,alt:"Image preview"},null,8,Z),(0,o.createElementVNode)("p",null,"图片路径: "+(0,o.toDisplayString)(i.imageUrl),1)])):(0,o.createCommentVNode)("",!0)])}a(62062);var I={props:{showPreview:{type:Boolean,default:!1},buttonText:{type:String,default:"選擇圖片"},backendUrl:{type:String,default:"BACKEND_URL"}},data(){return{imageFile:null,imageUrl:null}},methods:{triggerFileInput(){this.imageFile=null,this.imageUrl=null,this.$refs.fileInput.click()},handleFileChange(e){this.imageFile=e.target.files[0],this.imageFile&&(this.imageUrl=URL.createObjectURL(this.imageFile),this.$emit("OnFileUploaded",{imageFile:this.imageFile,imageUrl:this.imageUrl}))},async uploadToServer(){if(!this.imageFile)return void this.$swal.fire({text:"",title:"請先選擇圖檔",icon:"warning",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"});const e=new FormData;e.append("image",this.imageFile);try{const t=await fetch(this.backendUrl,{method:"POST",body:e});t.ok?this.$swal.fire({text:"",title:"上傳成功",icon:"info",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}):(console.log(t),this.$swal.fire({text:`${t.url} : ${t.status}-${t.statusText}`,title:"上傳失敗",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"}))}catch(t){console.error("上传错误:",t),this.$swal.fire({text:"",title:"上傳過程中發生錯誤",icon:"error",showCancelButton:!1,confirmButtonText:"OK",customClass:"my-sweetalert"})}}}},D=a(40089);const G=(0,D.Z)(I,[["render",A]]);var O=G,E=a(663),$={components:{ImageUploadButtonVue:O},data(){return{map:void 0,agv_feature:void 0,_agvBodyFeature:void 0,_agvSaftyRegionFeature:void 0,props:new _.MV,imageFile:null,imageUrl:null}},props:{agv_name:{type:String,default:"AGV_001"},raw_props:{type:_.MV,default(){var e=new _.MV;return e}}},computed:{map_id(){return"map-agv-preview-"+this.agv_name},featureStyle(){return this.agv_feature.getStyle()},iconUploadAPIEndPoint(){return`${E.Z.backend_host}/api/Map/AGVIconUpload?AGVName=${this.agv_name}`}},methods:{MapInit(){const e=new h.Z({code:"xkcd-image",units:"pixels",extent:[-20,-20,20,20]});this.agv_feature=new C.Z({geometry:new y.Z([0,0])}),this.agv_feature.setStyle((0,_.K$)(this.raw_props.DisplayText,this.raw_props.DisplayColor,"/images/AGVDisplayImage/"+this.agv_name+"-Icon.png"));const t=new V.ZP([[[-3,4],[3,4],[3,-4],[-3,-4]]]);this._agvBodyFeature=new C.Z(t),this._agvBodyFeature.setStyle(new S.ZP({fill:new F.Z({color:(0,_.xZ)(this.raw_props.DisplayColor,.6)}),stroke:new k.Z({color:"black",width:1})}));const a=new x.Z([0,0],5);this._agvSaftyRegionFeature=new C.Z(a);const o=(0,_.xZ)(this.raw_props.DisplayColor,.2);this._agvSaftyRegionFeature.setStyle(new S.ZP({fill:new F.Z({color:o}),stroke:new k.Z({color:o,width:1,lineDash:[5,2]})}));const l=new w.Z({source:new f.Z({features:[this.agv_feature,this._agvBodyFeature,this._agvSaftyRegionFeature]}),zIndex:2});this.map=new u.Z({layers:[l],target:this.map_id,view:new v.ZP({projection:e,center:[0,0],zoom:1,maxZoom:20})})},HandleAGVColorClicked(e){this.props.DisplayColor=e,this.ChangeColorOfAGV(e)},HandleAGVColorSelected(e){alert(e)},ChangeColorOfAGV(e){this.SetFeatureFillColor(this.agv_feature,e,!0),this.SetFeatureFillColor(this._agvBodyFeature,(0,_.xZ)(e,.4)),this.SetFeatureFillColor(this._agvSaftyRegionFeature,(0,_.xZ)(e,.2)),this.SaveToLocalStorage(!0)},ChangeTextOfAGV(e){var t=this.agv_feature.getStyle(),a=t.getText();a.setText(e),this.agv_feature.setStyle(t),this.SaveToLocalStorage()},SetFeatureFillColor(e,t,a=!1){var o=e.getStyle();if(a){var l=o.getText(),i=l.getBackgroundFill();i.setColor(t),l.setBackgroundFill(i)}else{i=o.getFill();i.setColor(t),o.setFill(i)}e.setStyle(o)},SaveToLocalStorage(e=!1){b.p.commit("SaveAGVStyle",{agvname:this.agv_name,style:this.props}),e&&setTimeout((()=>{B.Z.emit("/rerender_agv_layer")}),500)},HandleImageUploaded(e){console.info(e),this.imageFile=e.imageFile;var t=new Image;t.onload=()=>{var a=[t.width,t.height];this.agv_feature.setStyle((0,_.K$)(this.raw_props.DisplayText,this.raw_props.DisplayColor,e.imageUrl,a))},t.src=e.imageUrl}},mounted(){this.MapInit();var e=b.p.getters.CustomAGVStyles;e[this.agv_name]?this.props=e[this.agv_name]:this.props=JSON.parse(JSON.stringify(this.raw_props)),this.ChangeTextOfAGV(this.props.DisplayText),this.ChangeColorOfAGV(this.props.DisplayColor)}};const P=(0,D.Z)($,[["render",g],["__scopeId","data-v-443f0fb2"]]);var L=P,R={components:{AGVDrawSetting:L},methods:{SaveSetting(){}},computed:{agvNameList(){return N.sn.getters.AGVNameList}}};const K=(0,D.Z)(R,[["render",i]]);var M=K}}]);
//# sourceMappingURL=485.a3d5ca1f.js.map