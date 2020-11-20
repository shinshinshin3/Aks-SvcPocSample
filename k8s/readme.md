# 前提条件
- AKSでManagedIDが有効になっていること。  
(クラスタ作成時に--enable-managed-identityオプションをつけること)
- helmが有効になっていること

# Docker imageのPush
## ACRへログイン

~~~
# ACRname=funcregistry
# az acr login --name $ACRname
~~~

## docker imageのタグ付け
~~~
# docker tag aks-svcpocsample_bewebapi ${ACRname}.azurecr.io/aks_pocsvc_bewebapi:latest
# docker tag aks-svcpocsample_gwfunction ${ACRname}.azurecr.io/aks_pocsvc_gwfunc:latest
# docker tag aks-svcpocsample_gwwebapi ${ACRname}.azurecr.io/aks_pocsvc_gwwebapi:latest
~~~

## Docker imageのPush

~~~
# docker push  ${ACRname}.azurecr.io/aks_pocsvc_gwwebapi:latest
# docker push ${ACRname}.azurecr.io/aks_pocsvc_gwfunc:latest
# docker push ${ACRname}.azurecr.io/aks_pocsvc_bewebapi:latest
~~~


# 必要コンポーネントのインストール
下記3つのコンポーネントをインストールする
- Azure func tool
  - Azure functions をAKSにデプロイするためのクライアントツール。  
  - 自分の作業環境にインストールする
- csi secret store provider
  - AKSからKeyvaultを読むために必要なコンポーネント
  - AKSにhelmでデプロイする
- aad pod identity
  - AKSのPODにManagedIDを割り当てるために必要なコンポーネント
  - AKSにhelmでデプロイする
## Azurefunc toolのインストール

~~~
# curl -L https://github.com/Azure/azure-functions-core-tools/releases/download/2.7.2936/Azure.Functions.Cli.linux-x64.2.7.2936.zip >Azure.Functions.Cli.linux-x64.2.7.2936.zip
# unzip -d azure-functions-cli Azure.Functions.Cli.linux-x64.2.7.2936.zip
# mv azure-functions-cli/ /opt/
# cd /opt/azure-functions-cli/
# chmod 755  func
# chmod 755 gozip
# pushd /usr/local/bin
# ln -s /opt/azure-functions-cli/func .
# ln -s /opt/azure-functions-cli/gozip .
# popd
~~~

## csi secret store(AKSからKeyvaultを読むためのコンポーネント)のインストール

~~~
# helm repo add csi-secrets-store-provider-azure https://raw.githubusercontent.com/Azure/secrets-store-csi-driver-provider-azure/master/charts
# helm install csi-secrets-store-provider-azure/csi-secrets-store-provider-azure --generate-name
~~~

## POD Identity用のコンポーネントのインストール
~~~
# helm repo add aad-pod-identity https://raw.githubusercontent.com/Azure/aad-pod-identity/master/charts
# helm install pod-identity aad-pod-identity/aad-pod-identity
~~~


# name space作成
~~~
# SVC_NAMESPACE=poc-service
# kubectl create ns ${SVC_NAMESPACE}
~~~

# keyvaultの設定
Keyvaultを一つ作成し、dbの接続文字列とApplicationInsightsのkeyを設定する

## keyvaultの作成
~~~
# KEYVAULTNAME=aks-func-keyvault
# az keyvault create --name "aks-func-keyvault" --resource-group "aks-func-poc" --location japaneast
~~~

## secretの設定
~~~
dbConnectionString="Server=tcp:aks-poc-sqlserver.database.windows.net,1433;Initial Catalog=aks-poc-sqlserver;Persist Security Info=False;User ID=pocuser;Password=*******;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
ApplicationInsights_InstrumentationKey=a0f31******************10f3d90d8
az keyvault secret set --vault-name $KEYVAULTNAME --name "dbConnectionString" --value "$dbConnectionString"
az keyvault secret set --vault-name $KEYVAULTNAME --name "ApplicationInsightsInstrumentationKey" --value "$ApplicationInsights_InstrumentationKey"
~~~

# クラスタへの権限付与
AKSに設定されているManageIDに対して適切な許可設定を設定し、
PODに割り当てたIdentityの検証ができるようにする。

## Clientidを入手
~~~
# az aks show --name func-aks  --resource-group aks-func-poc --query identityProfile
# az account list --output table
# clientId=f3df7e*****************4737bc1
# RESOURCE_GROUP=aks-func-poc
★↓AKS作成時に自動的に作成されるノードリソースグループ
# NODE_RESOURCE_GROUP=MC_aks-func-poc_func-aks_japaneast
# SUBID=925a******************cb5304
~~~

## クラスタへの権限付与
~~~
# az role assignment create --role "Managed Identity Operator" --assignee $clientId --scope /subscriptions/$SUBID/resourcegroups/$RESOURCE_GROUP
# az role assignment create --role "Managed Identity Operator" --assignee $clientId --scope /subscriptions/$SUBID/resourcegroups/$NODE_RESOURCE_GROUP
# az role assignment create --role "Virtual Machine Contributor" --assignee $clientId --scope /subscriptions/$SUBID/resourcegroups/$NODE_RESOURCE_GROUP
~~~

# podに割り当てるManagedIDの作成と権限付与

PODに割り当てるManagedIDを作成し、clientidとprincipalidを環境変数に格納する
~~~
# az identity create -g $RESOURCE_GROUP -n aks-poc-pod-identity
{
  "clientId": "c6721***************d6c68f5bd",
  "clientSecretUrl": "https://control-japaneast.identity.azure.net/subscriptions/925a******************cb5304/resourcegroups/aks-func-poc/providers/Microsoft.ManagedIdentity/userAssignedIdentities/aks-poc-pod-identity/credentials?tid=b36f**********************62ff87d&oid=9f2c88*******************8ab2772&aid=c6721***************d6c68f5bd",
  "id": "/subscriptions/925a******************cb5304/resourcegroups/aks-func-poc/providers/Microsoft.ManagedIdentity/userAssignedIdentities/aks-poc-pod-identity",
  "location": "japaneast",
  "name": "aks-poc-pod-identity",
  "principalId": "9f2c88*******************8ab2772",
  "resourceGroup": "aks-func-poc",
  "tags": {},
  "tenantId": "b36f**********************62ff87d",
  "type": "Microsoft.ManagedIdentity/userAssignedIdentities"
}
# pod_clientid=c6721***************d6c68f5bd
# pod_principalid=9f2c88*******************8ab2772
~~~

## key vaultへのアクセス権限付与
作成したIDに対して、key vaultへのアクセス権限を付与する
~~~
# az role assignment create --role "Reader" --assignee $pod_principalid --scope /subscriptions/${SUBID}/resourceGroups/{RESOURCE_GROUP}/providers/Microsoft.KeyVault/vaults/aks-func-keyvault
# az keyvault set-policy -n aks-func-keyvault --secret-permissions get --spn $pod_clientid
# az keyvault set-policy -n aks-func-keyvault --key-permissions get --spn $pod_clientid
~~~

# Secret関連のk8sリソースを作成する
下記2つのリソースを作成する
両者とも、PODと同じNamespaceに作成する必要がある
- SecretProviderClass
  - PODにkeyvaultの情報を渡すためのリソース
- AzureIdentity
  - PODに割り当てるManagedIDの情報を渡すためのリソース
- Secret
  - k8sのSecret、Functionsに環境変数を渡すために利用する。

## SecretProviderClass を作成する

~~~
# az keyvault show --name "aks-func-keyvault" --query resourceGroup
# az keyvault show --name "aks-func-keyvault" --query tenantId
# vi aks-pocsvc-secretprovider.yaml
apiVersion: secrets-store.csi.x-k8s.io/v1alpha1
kind: SecretProviderClass
metadata:
  name: aks-poc-secret-provider
spec:
  provider: azure
  parameters:
    usePodIdentity: "true"
    useVMManagedIdentity: "false"
    userAssignedIdentityID: ""
    keyvaultName: "aks-func-keyvault"  # key vaultの名前
    objects:  |
      array:
        - |
          objectName: dbConnectionString  #ここに、提供するkeyを全部書く必要がある
          objectType: secret
        - |
          objectName: ApplicationInsightsInstrumentationKey
          objectType: secret
    resourceGroup: "poc-func-aks"  #
    subscriptionId: "925a******************cb5304"  # 上のコマンドで確認
    tenantId: "b36f**********************62ff87d"        # 上のコマンドで確認
  secretObjects:
  - data:
    - key: dbConnectionString
      objectName: dbConnectionString
    - key: ApplicationInsightsInstrumentationKey
      objectName: ApplicationInsightsInstrumentationKey
    secretName: aks-func-keyvault
    type: Opaque
# kubectl -n ${SVC_NAMESPACE} apply -f aks-pocsvc-secretprovider.yaml
~~~


## pod identityリソースを作成

~~~
# cat aks-pocsvc-pod-identity.yaml
apiVersion: aadpodidentity.k8s.io/v1
kind: AzureIdentity
metadata:
    name: "aks-poc-pod-identity"
spec:
    type: 0
    resourceID: /subscriptions/925a******************cb5304/resourceGroups/aks-func-poc/providers/Microsoft.ManagedIdentity/userAssignedIdentities/aks-poc-pod-identity  # 前に作ったAD IDのID
    clientID: "c6721***************d6c68f5bd"     # 前に作ったAD IDのclient ID
---
apiVersion: aadpodidentity.k8s.io/v1
kind: AzureIdentityBinding
metadata:
    name: aks-poc-pod-identity-binding
spec:
    azureIdentity: "aks-poc-pod-identity"
    selector: aks-poc-pod-identity-selector
kubectl -n ${SVC_NAMESPACE} apply -f aks-pocsvc-pod-identity.yaml
~~~

## secrets 作成

~~~
 # kubectl -n ${SVC_NAMESPACE} create secret generic aks-pocsvc-gwfunc-secret --from-env-file=aks-pocsvc-gwfunc-secret.env
~~~

# 各種POD作成
## func作成

~~~
# cd GwFunc01
# func kubernetes deploy --name aks-pocsvc-gwfunc --namespace ${SVC_NAMESPACE} --image-name funcregistry.azurecr.io/aks_pocsvc_gwfunc:latest --secret-name aks-pocsvc-gwfunc-secret --min-replicas 1
# kubectl -n ${SVC_NAMESPACE} edit deploy aks-pocsvc-gwfunc
編集1: labelの追加
      labels:
        app: aks-pocsvc-gwfunc
        aadpodidbinding: aks-poc-pod-identity-selector
編集2: envの追加
      - env:
        - name: AzureFunctionsJobHost__functions__0
          value: Functions
        - name: ApplicationInsights_InstrumentationKey
          valueFrom:
            secretKeyRef:
              name: aks-func-keyvault
              key: ApplicationInsightsInstrumentationKey
編集3: keyvaultのマウント
        terminationMessagePolicy: File
        volumeMounts:
        - name: secrets-store-inline
          mountPath: "/mnt/secrets-store"
          readOnly: true
      volumes:
        - name: secrets-store-inline
          csi:
            driver: secrets-store.csi.k8s.io
            readOnly: true
            volumeAttributes:
              secretProviderClass: "aks-poc-secret-provider"
      dnsPolicy: ClusterFirst
~~~

## その他deployment作成

~~~
# kubectl -n ${SVC_NAMESPACE} apply -f aks-pocsvc-beapp-deployment.yaml
# kubectl -n ${SVC_NAMESPACE} apply -f aks-pocsvc-beweb-deployment.yaml
# kubectl -n ${SVC_NAMESPACE} apply -f aks-pocsvc-gwweb-deployment.yaml
# kubectl -n ${SVC_NAMESPACE} apply -f aks-pocsvc-gwweb-ingress.yaml
~~~

## ingressのIPアドレスを確認

~~~
kubectl -n ${SVC_NAMESPACE} get ingress
NAME                       HOSTS   ADDRESS          PORTS   AGE
aks-pocsvc-ingress-gwapp   *       20.xxx.xxx.223   80      72s
~~~