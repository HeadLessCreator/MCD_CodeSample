# 🍽️ Eatable System (Unity C# Sample)

### ✅ 구조 설명
- `IEatable`: 먹을 수 있는 오브젝트를 위한 인터페이스
- `EatableBase`: 공통 로직 처리 (이펙트, 점수, 이벤트 발행 등)
- `EatableDog, EatableCivilian, EatableCar`: EatableBase 상속받은 클래스 (비명, 폭죽 이펙트 등)

### 🎯 설계 철학
인터페이스 기반 설계를 통해 유연성과 재사용성을 확보하였으며,  
FirstRespond / SecondRespond 두 추상 메서드로 개별 피드백 로직을 분리했습니다.

### 📁 폴더 구조
/EatableSystem/
├── IEatable.cs
├── EatableBase.cs
├── EatableDog.cs
├── EatableCivilian.cs
└── EatableCar.cs

## 🔗 For more info, see my Notion Portfolio [here](https://charm-root-c91.notion.site/1ed101f05bcb80f8b5aecd1354e6016d)
