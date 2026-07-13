---
slug: /references/components/network-id-assigner
---

# Network ID Assigner

アバターのビルド時に、PhysBone などのコンポーネントへネットワーク ID を割り当てるコンポーネントです。
アバターのルートオブジェクトに追加します。

PC と Mobile で PhysBone の揺れ方を同期させるには、両方のアバターで PhysBone が同じネットワーク ID を持つ必要があります。
このコンポーネントは、アバタールートからのヒエラルキーパスをもとに ID を決めるため、PC 用と Mobile 用で同じ構成であれば同じ ID が割り当てられます。

## 使い方

1. アバターのルートオブジェクトにこのコンポーネントを追加します。
2. PC 用と Mobile 用の両方のアバターをアップロードし直します。

設定項目はありません。

## 補足

[VQT Avatar Converter Settings](./avatar-converter-settings.md) の「Network ID を割り当てる」を有効にしている場合、このコンポーネントを別途追加する必要はありません。
