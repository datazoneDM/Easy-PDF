# PDF Split & Merge Tool (Server/Client)

이 프로젝트는 iText 7을 활용하여 구현된 PDF 분할 및 병합 프로그램입니다. 
비동기식 소켓 통신을 통해 하나의 프로그램 내에서 서버(Server)와 클라이언트(Client) 역할을 모두 수행할 수 있도록 설계되었습니다.

## 🛠 주요 기능 및 기술 스택
* **PDF 조작:** iText 7.1.5 / 7.1.16 (AGPLv3) 기반 PDF 병합 및 분할
* **네트워크 통신:** 비동기식 소켓(Asynchronous Socket) 통신을 이용한 서버/클라이언트 아키텍처
* **개발 환경:** Microsoft Visual Studio 2019
* **프레임워크:** .NET Framework (4.0/4.7 이상 권장)

## 🏗 빌드 및 실행 방법
1. 본 저장소의 전체 소스 코드를 다운로드(또는 Clone)합니다.
2. `Visual Studio 2019`를 통해 솔루션 파일을 엽니다.
3. NuGet 패키지 관리자를 통해 `itext7` 및 관련 종속성 라이브러리를 복원(Restore)합니다.
4. 솔루션을 빌드(Build)하여 실행 파일을 생성합니다.

## ⚖️ 라이선스 (License)

본 프로젝트는 **GNU Affero General Public License v3.0 (AGPL-3.0)**에 따라 라이선스가 부과됩니다.

### 중요 고지 사항:
1. **오픈 소스 준수:** 본 프로그램은 AGPLv3 라이선스를 따르는 iText 7 라이브러리를 사용합니다. 이에 따라, 본 프로그램의 소스 코드 역시 AGPLv3 조건에 의거하여 전체 공개됩니다.
2. **이용 의무:** 이 프로그램을 수정하거나 배포, 또는 네트워크를 통해 서비스를 제공하는 경우, 해당 수정본의 소스 코드 역시 AGPLv3 라이선스로 공개해야 합니다.
3. **상업적 이용:** 기업 사용자가 소스 코드 공개를 원치 않을 경우, [iText 공식 홈페이지](https://itextpdf.com/products/itextsharp)를 통해 별도의 상업용 라이선스(Commercial License)를 취득해야 합니다.

* **iText 7 License 정보:** [https://itextpdf.com/en/how-it-works/agpl-license](https://itextpdf.com/en/how-it-works/agpl-license)

## License

This project is licensed under the GNU Affero General Public License v3.0 (AGPL-3.0).

This project uses iText 7.

iText 7 is licensed under the GNU Affero General Public License (AGPL) v3.
Copyright (c) iText Software.

If you use this software over a network or distribute it,
you must provide access to the complete corresponding source code
as required by the AGPL license.

Source Code:
https://github.com/datazoneDM/Easy-PDF

For more information about iText licensing:
https://itextpdf.com
