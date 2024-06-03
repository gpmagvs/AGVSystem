rem Remote先設定github連結, 並且命名為 "github" 才能使用這個腳本

call git fetch
call git checkout master
call git pull github master 
cd ../

cd AGVS_UI
call git fetch
call git checkout master
call git pull github master 
cd ../

cd AGVSystemCommonNet6
call git fetch
call git checkout master
call git pull github master 
cd ../

cd EquipmentManagment
call git fetch
call git checkout master
call git pull github master 
cd ../

cd RosBridgeClient
call git fetch
call git checkout master
call git pull github master 
cd ../

cd vmsystem
call git fetch
call git checkout master
call git pull github master 
cd ../


pause