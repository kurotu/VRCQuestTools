# Network ID Assigner

ビルド時にPhysBoneにネットワークIDを割り当てます。
VRChat SDKとは異なりヒエラルキーパスのハッシュ値をもとにIDを割り当てるため、オブジェクトが増減しても同じIDが割り当てられます。

## NDMF

VRCQuestToolsプラグインによって以下の処理を実行します。

### Generating Phase

ネットワークIDのないPhysBoneにネットワークIDを割り当てます。
