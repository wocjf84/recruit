# ZIGBANG ZED Assignment

안녕하세요. 직방 ZED 팀 지원자 여러분. 지원해주셔서 감사합니다.

과제 설명 전 간단한 안내 말씀 드리겠습니다.

직방 ZED 팀은 유니티 엔진을 이용하여 직방에 있는 부동산 빅데이터들을

3D 정보로 시각화해주는 일을 하고 있습니다.

이와 관련하여 과제도 간단한 데이터를 이용해 3D 시각화로 진행하고 있습니다.

Github 의 fork 기능을 이용하여 개인 저장소로 복사해주신 다음 

코드를 작성 및 README.MD 파일 수정 후 개인 저장소의 URL을 채용 담당자에게 공유해주시면 됩니다.

최하단의 [문제 해결 방법] 섹션은 면접에 도움이 될 수 있도록

과제를 하면서 고민했던 부분, 해결방안 등에 대해 자유롭게 작성해주시면 됩니다.

## 과제 설명

- 과제는 유니티 2019.4.21f1 으로 작성합니다.

- Scenes/ZedScene.unity 씬에서 확인 가능하도록 작성부탁드립니다.

### 문제1) 단지에 있는 동의 폴리곤 데이터를 이용하여 런타임에 메쉬를 구성하여 렌더링해주세요.(40점)

샘플 데이터는 아래의 경로에 위치해 있습니다.

Assets/Samples/json/dong.json

다음과 같이 구성되어 있습니다.

```
API Response 구성
{
    success: boolean, // API 성공 여부
    code: number, // API 결과 코드
    data: [] // 동 데이터 배열
}

동 데이터 구성
{
    roomtypes: [], // 타입 데이터의 배열
    meta: {
        bd_id: number, // 동 id
        동: string, // 동 이름
        지면높이: number, // 지면으로 부터의 높이
    } // 메타 정보
}

타입 데이터 구성
{
    coordinatesBase64s: string[], // 폴리곤 데이터의 배열
    meta: {
        룸타입id: number // 타입 id
    }
}

폴리곤 데이터의 구성 (*중요*)
- float[] 을 byte[] 로 변환후 Base64 로 인코딩된 데이터
- float[] 는 3개씩 나누어 Vector3 의 x,z,y 에 매핑됩니다.
  ex) [1.0, 2.0, 3.0, 4.0, 5.0, 6.0]
  -> [Vector3(1.0, 3.0, 2.0), Vector3(4.0, 6.0, 5.0)]
- C# 의 Buffer.BlockCopy, Convert.FromBase64String 등의 함수를 이용하시면 됩니다.
```
**폴리곤 데이터 1당 유니티 좌표계의 1m 를 의미합니다.**

아래의 그림과 유사한 단지 구성이 되어야 합니다. 평형별 색상은 고려하지 않으셔도 됩니다.

![결과1](https://i.imgur.com/vcmmiid.png)



### 문제2) 문제1의 결과를 바탕으로 생성된 메쉬에 텍스쳐를 매핑합니다.(60점)

텍스쳐의 위치는 아래와 같습니다.

Assets/Samples/texture/buildingTester_d.png

텍스쳐는 아래와 같이 구성되어 있습니다.
![Texture](https://i.imgur.com/srIPHBq.png)


텍스쳐 매핑은 다음과 같습니다.

1번(앞) : 정점의 normal과 Vector3.forward의 각도가 y축을 기준으로 180 <= n && n <= 220 에 해당하는 경우

2번 : 1, 3을 제외한 모든 경우

3번(위, 아래) : 정점의 normal이 y축을 기준으로 Vector3.up || Vector3.down과 일치하는 경우


텍스쳐 파일을 자르지 않고 한장으로 그려야합니다. 

**한 층을 3m 로 가정하여 Material 의 SetTextureScale 값을 조정해주세요.**

**ex) 한 동의 높이가 10m 인 경우 = 10 / 3 = 3.3333 = 3층**

**Material.SetTextureScale("_BaseMap", new Vector2(1f, 3));**

아래의 그림과 비슷한 모양이 나와야합니다. 실 데이터와 차이가 있으므로 정확하게 일치하지는 않습니다.

![결과2](https://i.imgur.com/gbjA93E.png)

#### 평가기준

1. <b>아파트 단지에 텍스쳐 맵핑을 적용 (10점)

2. 주어진 텍스쳐 이미지를 문제에서 제시한 영역만큼 구분하여 적용 (20점)
   * 정점의 uv 좌표를 이용하여 구현해야 합니다. 텍스쳐 파일 나누어 구현하는 것은 인정되지 않습니다.

3. 적용된 텍스쳐를 문제에 제시한 방향 조건에 맞추어 적용 (30점)</b>

### 코드 설계시 우대되는 부분

유니티 라이프 사이클 함수 (Awake, Start, OnEnable, OnDisable, OnDestroy, Update) 이외의 직접 라이플 사이클을 구현 <b>(점수 획득한 문제에 가산점 +20점)</b>

MyFramework.cs 파일에 GameObject / Material 등의 리소스 생성 및 삭제가 구현되어야 합니다.

MyFramework.cs 파일에만 유니티 라이프 사이클 함수를 사용합니다.

리소스 생성 및 삭제의 순서가 명확해야합니다.

# 문제 해결 방법 (자유롭게 작성)

바닥 및 천장의 vertex 순서와 벽면의 vertex 순서가 달라 UV 작업 시 순서를 제대로 지정하기 어려웠습니다.

각 좌표마다 Sphere를 순서대로 넣어 vetex 순서를 확인 후 벽면 과 바닥 천장을 구분하여 UV를 매핑하였습니다.

또한, 건물 중 한 동에 다른 높이의 건물이 있는 경우가 있어서 매핑을 위해 Mesh를 따로 생성하였습니다.

처음 작업할때 Material을 유니티 상에 만들어 작업을 진행했고, Mesh가 아닌 ProbuilderMesh를 사용하여 작업했지만, 원하는대로 매핑이 되지 않아 Mesh로 변경하였고,
과제 내용을 확인 후 Material 또한 코드에서 생성하도록 변경했습니다.

벽면의 Normal은 쉽게 구할 수 있었지만, 건물의 Normal을 구할 기준을 제대로 잡지 못해 몇몇 건물의 경우 아직 제대로 매핑이 되지 않고 있습니다.

여러가지 추가작업(동 표시, 각 Mesh의 좌표 설정)을 하고 싶었지만 제출이 더 중요하다 판단 해 우선 제출하기로 하였고, 긍정적인 평가를 받게되어 합격이 되면, 반드시 완성 시켜야 할 작업이라 생각 합니다.

감사합니다.
