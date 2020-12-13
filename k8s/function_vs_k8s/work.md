# Azure Functions vs k8s

## Web Apps Create
Operate Azure Portal.
Add Application Settings use Azure CLI
~~~
az webapp config appsettings set -g aks-func-poc -n poc-Gwfunc01 --settings TOPIC=accident
az webapp config appsettings set -g aks-func-poc -n poc-Gwfunc01 --settings CONSUMER_GROUP=GwFunction
az webapp config appsettings set -g aks-func-poc -n poc-Gwfunc01 --settings BROKER_LIST=10.0.1.5:9092
az webapp config appsettings set -g aks-func-poc -n poc-Gwfunc01 --settings BackEnd_URL=http://aks-pocsvc-beweb/api/AccidentHistory
az webapp config appsettings set -g aks-func-poc -n poc-Gwfunc01 --settings BackEnd_URL=http://20.44.184.46/api/AccidentHistory
az webapp config appsettings set -g aks-func-poc -n poc-Gwfunc01 --settings Log_Level=Information
az webapp config appsettings set -g aks-func-poc -n poc-Gwfunc01 --settings KafkaExtension_MaxBatchSize=128
az webapp config appsettings set -g aks-func-poc -n poc-Gwfunc01 --settings KafkaExtension_SubscriberIntervalInSeconds=1 
az webapp config appsettings set -g aks-func-poc -n poc-Gwfunc01 --settings KafkaExtension_ExecutorChannelCapacity=3
az webapp config appsettings set -g aks-func-poc -n poc-Gwfunc01 --settings KafkaExtension_ChannelFullRetryIntervalInMs=50
~~~

## Application Insights Query

kafkaEvent array length per function execution.
~~~
traces
| where message contains "Length"
| extend KafkaEventLength = toint(extract("[0-9]+", 0, message))
| summarize sum(KafkaEventLength) by bin(timestamp, 1s), cloud_RoleInstance
~~~

function execution count by 1sec
~~~
traces
| where cloud_RoleInstance contains "func" or cloud_RoleInstance contains "RD"
| where message contains "Executed"
| summarize count() by bin(timestamp, 1s), cloud_RoleInstance
~~~

function data count by 1sec
~~~
traces
| where cloud_RoleInstance contains "func" or cloud_RoleInstance contains "RD"
| where message contains "TransactionId"
| extend tid = parse_json(message)
| summarize count(tid) by bin(timestamp, 1s), cloud_RoleInstance
~~~


Add NodePool 
~~~
az aks nodepool add \
    --resource-group aks-func-poc \
    --cluster-name func-aks \
    --name nplix2 \
    --node-vm-size Standard_DS2_v2 \
    --node-count 2

az aks nodepool add \
    --resource-group aks-func-poc \
    --cluster-name func-aks \
    --os-type Windows \
    --name npwin \
    --node-vm-size Standard_DS2_v2 \
    --node-count 2

~~~

