"use strict";(self["webpackChunkgpm_agvs"]=self["webpackChunkgpm_agvs"]||[]).push([[27],{86117:function(e,r,t){t.d(r,{Z:function(){return w}});var s=t(79003);const A=["onMousemove"],d=["onMousemove"];function i(e,r,t,i,a,c){return(0,s.openBlock)(),(0,s.createElementBlock)("div",null,[(0,s.createElementVNode)("div",{class:"table-selector",onMouseleave:r[0]||(r[0]=()=>{a.selectedRows=a.selectedCols=0}),onMousemove:r[1]||(r[1]=(...r)=>e.handleMouseMove&&e.handleMouseMove(...r)),onClick:r[2]||(r[2]=(...e)=>c.handleClick&&c.handleClick(...e))},[((0,s.openBlock)(!0),(0,s.createElementBlock)(s.Fragment,null,(0,s.renderList)(t.maxColNum,(e=>((0,s.openBlock)(),(0,s.createElementBlock)("div",{key:e,onMousemove:r=>c.HandleMoseMoveOnRow(r,e)},[((0,s.openBlock)(!0),(0,s.createElementBlock)(s.Fragment,null,(0,s.renderList)(t.maxRowNum,(r=>((0,s.openBlock)(),(0,s.createElementBlock)("div",{class:(0,s.normalizeClass)(["cell d-flex",{selected:c.isSelected(e,r)}]),key:r,onMousemove:e=>c.HandleMoseMoveOnColumn(e,r)},null,42,d)))),128))],40,A)))),128))],32),(0,s.createElementVNode)("p",null,"選擇了 "+(0,s.toDisplayString)(a.selectedRows)+" 行 和 "+(0,s.toDisplayString)(a.selectedCols)+" 列 的表格",1)])}var a={props:{maxRowNum:{type:Number,default:6},maxColNum:{type:Number,default:10}},data(){return{selectedRows:0,selectedCols:0,hoverIndex:0}},methods:{isSelected(e,r){return r<=this.selectedRows&&e<=this.selectedCols},HandleMoseMoveOnRow(e,r){e.target;this.selectedCols=r},HandleMoseMoveOnColumn(e,r){e.target;this.selectedRows=r},handleClick(){this.$emit("on-selected",{row:this.selectedRows,col:this.selectedCols})}}},c=t(40089);const o=(0,c.Z)(a,[["render",i],["__scopeId","data-v-1578eb4a"]]);var w=o},90451:function(e,r,t){t.d(r,{Z:function(){return x}});var s=t(79003);const A={class:"rack p-3"};function d(e,r,t,d,i,a){const c=(0,s.resolveComponent)("RackPortImageVue");return(0,s.openBlock)(),(0,s.createElementBlock)("div",A,[((0,s.openBlock)(!0),(0,s.createElementBlock)(s.Fragment,null,(0,s.renderList)(a.rowAry,(e=>((0,s.openBlock)(),(0,s.createElementBlock)("div",{class:"d-flex flex-row",key:"row-"+e},[((0,s.openBlock)(!0),(0,s.createElementBlock)(s.Fragment,null,(0,s.renderList)(a.colAry,(r=>((0,s.openBlock)(),(0,s.createBlock)(c,{key:"col-"+r,class:(0,s.normalizeClass)(r>1?"shit-left":""),col:r,row:e,hasRack:a.CargoExist(r,e),CarrierID:a.CarrierID(r,e),onOnPortClick:a.HandlePortClicked,isBottom:0==e},null,8,["class","col","row","hasRack","CarrierID","onOnPortClick","isBottom"])))),128))])))),128))])}t(57658);var i="data:image/png;base64,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",a="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAATMAAAE7CAMAAACG1ZYBAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAPUExURQQkM9nZ2TZPWn9/fwAAACWTLiwAAAAFdFJOU/////8A+7YOUwAAAAlwSFlzAAAywAAAMsABKGRa2wAABMRJREFUeF7t2rGOFTAUxNAF/v+fmSc5ElVwCrbBp5zS1ZWSryRJkm/wI39Hq4M1N7Q6WHNDq4M1N7Q6WHNDq4M1N7Q6WHNDq4M1N7Q6WHNDq4M1N7Q6WHNDq4P142f+RJUPWh2sHz+Z8lGzdzV7V7N3NXtXs3c1e/fW7Nd/jARTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEc2mWJEnyL3GB5IZWB2tuaHWw5oZWB2tuaHWw5oZWB2tuaHWw5oZWB2tuaHWw5oZWSZIk34EL5OO8b+bj7U04HzV7198DiwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBFMziwRTM4sEUzOLBPPWLB81e1ezdzV7V7N3NXtXs3c1eyeb5U9U+aDVwZobWh2suaHVwZobWh2suaHVwZobWh2suaHVwZobWh2suaHVwZobWh2suaHVwZobWiVJkvw7X1+/AQrO+r6NFracAAAAAElFTkSuQmCC",c="data:image/png;base64,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",o="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAasAAAHACAMAAAAr/LfGAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAABsUExURQAAAAAAAAAcOQArOQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMkMgAAAAAAAAAAAAAAAAAAAAAAAAQkNAQkMwAAAAAAAAAAAAAAAAAAAAEFCAENEgIRGAMfLAQjMwQkMxkk28IAAAAddFJOUwAJEhIWIzA9QEpYZXJ/gIyTmaaztL/BxsfO2+j11oxMwwAAAAlwSFlzAAAywAAAMsABKGRa2wAAC+JJREFUeF7t2OuSHMUBRGEMBt+wjbGN8GW57fu/o4ueI3bVW05tzfSg7ND5+KPt6ii18gQRCn3yoLOw1XnY6jxsdR62Og9bnYetzsNW52Gr87DVedjqPGx1HrY6D1udh63Ow1bnYavzsNV52Oo8bHUetjoPW52Hrc7DVudhq/Ow1XnY6jxsdR62Oo/XtPpE98fWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJrUqwdWKrEmyd2KoEWye2KsHWia1KsHViqxJsndiqBFsntirB1omtSrB1YqsSbJ3YqgRbJ7YqwdaJ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";const w=e=>((0,s.pushScopeId)("data-v-45edce5f"),e=e(),(0,s.popScopeId)(),e),q={key:0,class:"rack-port rack-bottom-port",style:{width:"170px"}},l=w((()=>(0,s.createElementVNode)("img",{class:"port-img",src:i,width:"170"},null,-1))),B={key:0,class:"rack-tray-img",src:a,width:"170"},W={key:1,class:"rack-tray-img",src:c,width:"170"},M={key:1,class:"rack-port",style:{width:"170px"}},V=w((()=>(0,s.createElementVNode)("img",{class:"port-img",src:o,width:"170"},null,-1))),n={key:0,class:"rack-tray-img",src:a,width:"170"},O={key:1,class:"rack-tray-img",src:c,width:"170"},u={key:2,class:"qr-code d-flex flex-column"},z=w((()=>(0,s.createElementVNode)("i",{class:"bi bi-qr-code"},null,-1))),m=w((()=>(0,s.createElementVNode)("label",{for:""},"0",-1))),E=[z,m],b=w((()=>(0,s.createElementVNode)("div",{class:"selected-border",style:{width:"100%",height:"170px",position:"absolute",top:"0"}},null,-1)));function H(e,r,t,A,d,i){const a=(0,s.resolveComponent)("el-tag");return(0,s.openBlock)(),(0,s.createElementBlock)("div",{onClick:r[0]||(r[0]=(...e)=>i.PortClick&&i.PortClick(...e)),class:(0,s.normalizeClass)(["rack-port-container",t.colBeSelected?"rack-col-selected":""]),style:(0,s.normalizeStyle)({position:"relative",top:12*t.row+"px",left:-15*t.col+"px"})},[t.isBottom?((0,s.openBlock)(),(0,s.createElementBlock)("div",q,[l,t.hasRack?((0,s.openBlock)(),(0,s.createElementBlock)("img",B)):(0,s.createCommentVNode)("",!0),t.hasTray?((0,s.openBlock)(),(0,s.createElementBlock)("img",W)):(0,s.createCommentVNode)("",!0)])):((0,s.openBlock)(),(0,s.createElementBlock)("div",M,[V,t.hasRack?((0,s.openBlock)(),(0,s.createElementBlock)("img",n)):(0,s.createCommentVNode)("",!0),t.hasTray?((0,s.openBlock)(),(0,s.createElementBlock)("img",O)):(0,s.createCommentVNode)("",!0)])),(0,s.withDirectives)((0,s.createVNode)(a,{effect:"dark"},{default:(0,s.withCtx)((()=>[(0,s.createTextVNode)((0,s.toDisplayString)(t.CarrierID),1)])),_:1},512),[[s.vShow,t.CarrierID&&""!=t.CarrierID]]),0==t.row?((0,s.openBlock)(),(0,s.createElementBlock)("div",u,E)):(0,s.createCommentVNode)("",!0),b],6)}var Y={props:{isBottom:{type:Boolean,default:!1},col:{type:Number,default:0},row:{type:Number,default:0},colBeSelected:{type:Boolean,dafault:!0},hasRack:{type:Boolean,dafault:!1},hasTray:{type:Boolean,dafault:!1},CarrierID:{type:String,default:""}},data(){return{}},methods:{PortClick(){this.$emit("on-port-click",{row:this.row,col:this.col,hasCargo:this.hasRack||this.hasTray,carrierID:this.CarrierID})}}},v=t(40089);const y=(0,v.Z)(Y,[["render",H],["__scopeId","data-v-45edce5f"]]);var R=y,K=t(24239),F={components:{RackPortImageVue:R},props:{rackName:{type:String,default:""},isEdit:{type:Boolean,default:!1},editProps:{type:Object,default(){return{col:1,row:1}}}},data(){return{col:3,row:3}},computed:{colOfRack(){return this.isEdit?this.editProps.col:this.RackData?this.RackData.Columns:0},rowOfRack(){return this.isEdit?this.editProps.row:this.RackData?this.RackData.Rows:0},rowAry(){const e=[];for(let r=this.rowOfRack-1;r>=0;r--)e.push(r);return e},colAry(){const e=[];for(let r=0;r<this.colOfRack;r++)e.push(r);return e},RacksData(){return K.jU.getters.WIPData},RackData(){return this.RacksData.find((e=>e.WIPName==this.rackName))}},methods:{HandlePortClicked(e){alert(JSON.stringify(e))},GetPortByColRow(e,r){if(this.RackData&&this.RackData.Ports)return this.RackData.Ports.find((t=>t.Properties.Row==r&&t.Properties.Column==e))},CargoExist(e,r){if(this.isEdit)return!1;var t=this.GetPortByColRow(e,r);return!!t&&t.CargoExist},CarrierID(e,r){if(this.isEdit)return"";var t=this.GetPortByColRow(e,r);return!!t&&t.CarrierID}},mounted(){}};const g=(0,v.Z)(F,[["render",d],["__scopeId","data-v-2017133c"]]);var x=g}}]);
//# sourceMappingURL=27.37088bb2.js.map