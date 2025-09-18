using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Yejun.UGUI
{
    [AddComponentMenu("UI/Infinite Scroll Rect", 37)]
    public class InfiniteScrollRect : ScrollRect
    {
        // 인덱스 유효성을 검증하는 콜백 함수 (무한 스크롤 시 다음/이전 인덱스가 유효한지 확인)
        public Func<int, bool> onVerifyIndex;

        [SerializeField]
        // 버퍼 공간 (스크롤 시 아이템을 미리 로드하는 여유 공간)
        private float m_buffer = 100f;

        [SerializeField]
        // 비활성화된 아이템을 자동으로 처리할지 여부
        private bool m_autoInactive = false;

        [SerializeField]
        // 루프 모드 사용 여부 (무한 스크롤)
        private bool m_loopMode = true;

        [SerializeField]
        // 스냅샷 모드 사용 여부 (특정 위치로 스냅되는 스크롤)
        private bool m_snapshotMode = false;

        /// <summary>
        /// Key: Content (RectTransform)
        /// Value: Index
        /// </summary>
        // 현재 스크롤 뷰에 표시되는 컨텐츠(아이템)와 해당 인덱스를 매핑하는 딕셔너리
        private Dictionary<RectTransform, int> m_contents;
        // 컨텐츠(아이템)의 RectTransform 배열 (스냅샷 모드에서 사용)
        private RectTransform[] m_contents2;
        // 비활성화될 아이템들을 저장하는 리스트 (m_autoInactive가 true일 때 사용)
        private List<RectTransform> m_autoInactives;
        // 드래그 중인 경우 스크롤 위치 보정을 위한 델타 값
        private Vector2 m_delta;
        // 드래그 중인지 여부
        private bool m_isDrag;
        // 컨텐츠 업데이트가 필요한지 여부
        private bool m_isUpdate;
        // 현재 스크롤 뷰에 표시되는 아이템 중 가장 작은 인덱스
        private int m_indexMin;
        // 현재 스크롤 뷰에 표시되는 아이템 중 가장 큰 인덱스 + 1
        private int m_indexMax;
        // Content에 붙어있는 LayoutGroup 컴포넌트
        private LayoutGroup m_layoutGroup;
        
        [SerializeField]
        private GameObject m_prefab; // 생성할 프리팹
        private float m_itemHeight; // 프리팹(아이템)의 높이

        protected override void Awake()
        {
            base.Awake();

            // ValueChanged 이벤트 리스너 등록
            onValueChanged.AddListener(OnValueChanged);

            m_autoInactives = new List<RectTransform>();
            m_contents = new Dictionary<RectTransform, int>();
            // content의 자식 개수만큼 배열 초기화
            m_contents2 = new RectTransform[content.childCount];

            // Content의 LayoutGroup 컴포넌트 가져오기
            m_layoutGroup = content.GetComponent<LayoutGroup>();
            
            if(Application.isPlaying) // 이유는 모르나 edit모드에서도 호출되기에 수정
                Init();
        }

        // 스크롤 뷰 초기화
        private void Init()
        {
            m_contents.Clear();

            // 프리팹이 설정되지 않았으면 경고
            if (m_prefab == null)
            {
                Debug.LogError("InfiniteScrollRect: Prefab is not assigned!");
                return;
            }

            // 프리팹의 RectTransform을 가져와 높이를 저장 (한 번만 계산)
            // Layout Group이 적용된 경우, LayoutElement에서 preferredHeight를 사용하거나,
            // Canvas.ForceUpdateCanvases() 후 RectTransform.rect.height를 사용해야 정확할 수 있음
            // 여기서는 간단하게 Prefab의 RectTransform.rect.height를 사용합니다.
            RectTransform prefabRect = m_prefab.GetComponent<RectTransform>();
            if (prefabRect == null)
            {
                Debug.LogError("InfiniteScrollRect: Prefab does not have a RectTransform component!");
                return;
            }
            m_itemHeight = prefabRect.rect.height;

            // content의 높이에 맞춰 생성할 아이템 개수 계산
            // content.rect.height는 현재 content의 높이이므로, InfiniteScrollRect가 관리하는 아이템이 많아지면 늘어남
            // 초기 뷰포트 크기를 기준으로 계산하는 것이 더 적절할 수 있습니다.
            // 여기서는 초기 content의 높이를 기준으로 합니다.
            int initialItemCount = Mathf.CeilToInt(viewport.rect.height / m_itemHeight) + 2; // 최소한 뷰포트를 채우고 위아래로 여유분 추가
            
            // 기존 자식 오브젝트 제거 (초기화 시)
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                //Destroy(content.GetChild(i).gameObject);
                DestroyImmediate(content.GetChild(i).gameObject);
            }

            // 필요한 개수만큼 프리팹 생성 및 content에 추가
            m_contents2 = new RectTransform[initialItemCount]; // m_contents2 배열 크기 재설정
            for (int i = 0; i < initialItemCount; i++)
            {
                GameObject newItem = Instantiate(m_prefab, content);
                RectTransform newRect = newItem.GetComponent<RectTransform>();
                m_contents.Add(newRect, i);
                m_contents2[i] = newRect; // m_contents2 배열에도 추가
            }

            // 초기 최소/최대 인덱스 설정
            m_indexMin = 0;
            m_indexMax = content.childCount;

            // 세로 스크롤이면 최상단으로 이동
            if (vertical)
            {
                SetNormalizedPosition(1, 1);
            }

            // 가로 스크롤이면 최좌측으로 이동
            if (horizontal)
            {
                SetNormalizedPosition(0, 0);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // 이벤트 리스너 제거
            onValueChanged.RemoveAllListeners();
            // 콜백 함수 초기화
            onVerifyIndex = default;
            // 딕셔너리 클리어 및 초기화
            m_contents?.Clear();
            m_contents = default;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // 활성화될 때 컨텐츠 업데이트 필요 플래그 설정
            m_isUpdate = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // 비활성화될 때 드래그 상태 초기화
            m_isDrag = false;
        }

        private void Update()
        {
            // 자동 비활성화 모드이고 비활성화될 아이템이 있다면 처리
            if (m_autoInactive && m_autoInactives.Any())
            {
                foreach (var target in m_autoInactives)
                {
                    // onVerifyIndex 콜백을 통해 활성화 여부 결정
                    target.gameObject.SetActive(onVerifyIndex?.Invoke(m_contents[target]) ?? true);

                    // IContent 인터페이스를 구현한 컴포넌트에 업데이트 이벤트 실행
                    ExecuteEvents.Execute<IContent>(target.gameObject, null, (handler, data) =>
                    {
                        handler.Update(m_contents[target]);
                    });
                }
                m_autoInactives.Clear(); // 처리 후 리스트 비우기
            }

            // 컨텐츠 업데이트가 필요하면 업데이트 함수 호출
            if (m_isUpdate)
            {
                m_isUpdate = false;
                UpdateContent();
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            // 드래그 시작 시 드래그 중 플래그 설정
            m_isDrag = true;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            // 드래그 종료 시 델타 값 초기화 및 드래그 중 플래그 해제
            m_delta = Vector2.zero;
            m_isDrag = false;
            base.OnEndDrag(eventData);
        }

        protected override void SetContentAnchoredPosition(Vector2 position)
        {
            // 계산 순서상 반드시 base보다 먼저 해야함 (드래그 중 위치 보정)
            position.x -= m_delta.x;
            position.y -= m_delta.y;
            base.SetContentAnchoredPosition(position);
        }

        // 모든 컨텐츠(아이템) 업데이트
        private void UpdateContent()
        {
            if (m_contents == default)
            {
                return;
            }

            for (int i = 0; i < content.childCount; i++)
            {
                RectTransform target = (RectTransform)content.GetChild(i);

                // 자동 비활성화 모드에 따라 활성화 여부 결정
                if (m_autoInactive)
                {
                    target.gameObject.SetActive(onVerifyIndex?.Invoke(m_contents[target]) ?? true);
                }
                else
                {
                    target.gameObject.SetActive(true);
                }

                // IContent 인터페이스를 구현한 컴포넌트에 업데이트 이벤트 실행
                ExecuteEvents.Execute<IContent>(target.gameObject, null, (handler, data) =>
                {
                    handler.Update(m_contents[target]);
                });
            }
        }

        // 스크롤 값 변경 시 호출되는 콜백 함수
        private void OnValueChanged(Vector2 value)
        {
            // 루프 모드일 경우 루프 업데이트 함수 호출
            if (m_loopMode)
            {
                UpdateVerticalLoopMode();
                UpdateHorizontalLoopMode();
            }
            // 스냅샷 모드일 경우 스냅샷 업데이트 함수 호출
            else if (m_snapshotMode)
            {
                UpdateVerticalSnapshotMode();
            }
        }

        // 세로 루프 모드 업데이트
        private void UpdateVerticalLoopMode()
        {
            if (!vertical) // 세로 스크롤이 아니면 리턴
            {
                return;
            }

            // 위로 스크롤 중일 때
            if (velocity.y > 0)
            {
                // 뷰포트 상단 밖으로 벗어난 아이템들을 찾음
                var targets = m_contents.Where(t =>
                {
                    bool result = t.Key.offsetMin.y > content.InverseTransformPoint(viewport.position).y + viewport.rect.yMax + m_buffer;
                    return result;
                }).OrderBy(t => t.Value); // 인덱스 순으로 정렬

                bool isUpdated = targets.Any(t =>
                {
                    // 다음 인덱스가 유효한지 확인
                    var result = onVerifyIndex?.Invoke(t.Value + content.childCount) ?? true;
                    return result;
                });

                if (isUpdated)
                {
                    // 현재 아이템 중 가장 큰 인덱스
                    int lastIndex = m_contents.Max(t => t.Value);
                    foreach (var target in targets)
                    {
                        // 아이템의 인덱스를 다음 인덱스로 업데이트
                        m_contents[target.Key] = ++lastIndex;
                        // 아이템을 맨 마지막 자식으로 이동
                        target.Key.SetAsLastSibling();

                        // 자동 비활성화 모드에 따라 처리
                        if (m_autoInactive)
                        {
                            m_autoInactives.Add(target.Key);
                        }
                        else
                        {
                            target.Key.gameObject.SetActive(true);

                            // IContent 인터페이스를 구현한 컴포넌트에 업데이트 이벤트 실행
                            ExecuteEvents.Execute<IContent>(target.Key.gameObject, null, (handler, data) =>
                            {
                                handler.Update(lastIndex);
                            });
                        }
                    }

                    // 스크롤 위치 보정
                    Vector3 pos = content.position;
                    RectTransform child = (RectTransform)content.GetChild(0); // 첫 번째 자식
                    Vector2 childWorldTopPos = content.TransformPoint(child.offsetMax);
                    Vector3 localPos = content.parent.InverseTransformPoint(childWorldTopPos);

                    if (m_isDrag)
                    {
                        m_delta += content.offsetMax - (Vector2)localPos;
                    }

                    localPos.x = content.localPosition.x; // x 위치는 유지
                    content.localPosition = localPos; // content 위치 업데이트
                }
            }
            // 아래로 스크롤 중일 때
            else if (velocity.y < 0)
            {
                // 뷰포트 하단 밖으로 벗어난 아이템들을 찾음
                var targets = m_contents.Where(t =>
                {
                    bool result = t.Key.offsetMax.y < content.InverseTransformPoint(viewport.position).y + viewport.rect.yMin - m_buffer;
                    return result;
                }).OrderByDescending(t => t.Value); // 인덱스 역순으로 정렬

                bool isUpdated = targets.Any(t =>
                {
                    // 이전 인덱스가 유효한지 확인
                    var result = onVerifyIndex?.Invoke(t.Value - content.childCount) ?? true;
                    return result;
                });

                if (isUpdated)
                {
                    // 현재 아이템 중 가장 작은 인덱스
                    int firstIndex = m_contents.Min(t => t.Value);
                    foreach (var target in targets)
                    {
                        // 아이템의 인덱스를 이전 인덱스로 업데이트
                        m_contents[target.Key] = --firstIndex;
                        // 아이템을 맨 앞 자식으로 이동
                        target.Key.SetAsFirstSibling();

                        // 자동 비활성화 모드에 따라 처리
                        if (m_autoInactive)
                        {
                            m_autoInactives.Add(target.Key);
                        }
                        else
                        {
                            target.Key.gameObject.SetActive(true);

                            // IContent 인터페이스를 구현한 컴포넌트에 업데이트 이벤트 실행
                            ExecuteEvents.Execute<IContent>(target.Key.gameObject, null, (handler, data) =>
                            {
                                handler.Update(firstIndex);
                            });
                        }
                    }

                    // 스크롤 위치 보정
                    RectTransform child = (RectTransform)content.GetChild(content.childCount - 1); // 마지막 자식
                    Vector2 childWorldButtomPos = content.TransformPoint(child.offsetMin);
                    Vector3 localPos = content.parent.InverseTransformPoint(childWorldButtomPos);

                    if (m_isDrag)
                    {
                        m_delta += content.offsetMin - (Vector2)localPos;
                    }

                    localPos.x = content.localPosition.x; // x 위치는 유지
                    localPos.y += content.rect.height; // content 높이만큼 이동 (아래로 스크롤 시)

                    content.localPosition = localPos; // content 위치 업데이트
                }
            }
        }

        // 세로 스냅샷 모드 업데이트
        private void UpdateVerticalSnapshotMode()
        {
            // 위로 스크롤 중일 때
            if (velocity.y > 0)
            {
                // 뷰포트 상단 밖으로 벗어난 아이템들을 찾음
                var targets = m_contents2.Where(t =>
                {
                    bool result = t.offsetMin.y > content.InverseTransformPoint(viewport.position).y + viewport.rect.yMax + m_buffer;
                    return result;
                });

                bool isUpdated = false;

                // 다음 인덱스들이 유효한지 확인
                for (int i = m_indexMax + 1; i <= m_indexMax + targets.Count(); i++)
                {
                    isUpdated = onVerifyIndex?.Invoke(i) ?? true;
                    if (isUpdated)
                    {
                        break;
                    }
                }

                if (isUpdated)
                {
                    // 최소/최대 인덱스 업데이트
                    m_indexMin += targets.Count();
                    m_indexMax += targets.Count();

                    // 모든 아이템의 데이터 업데이트
                    for (int targetIndex = 0, dataIndex = m_indexMin; targetIndex < m_contents2.Length; targetIndex++, dataIndex++)
                    {
                        RectTransform target = m_contents2[targetIndex];

                        // 자동 비활성화 모드에 따라 활성화 여부 결정 및 업데이트 이벤트 실행
                        target.gameObject.SetActive(!m_autoInactive || (onVerifyIndex?.Invoke(dataIndex) ?? true));

                        ExecuteEvents.Execute<IContent>(target.gameObject, null, (handler, data) =>
                        {
                            handler.Update(dataIndex);
                        });
                    }

                    // 스크롤 위치 보정
                    Vector3 pos = content.position;
                    RectTransform child = (RectTransform)content.GetChild(targets.Count()); // targets.Count() 만큼 이동한 위치의 자식
                    Vector2 childWorldTopPos = content.TransformPoint(child.offsetMax);
                    Vector3 localPos = content.parent.InverseTransformPoint(childWorldTopPos);

                    if (m_isDrag)
                    {
                        m_delta += content.offsetMax - (Vector2)localPos;
                    }

                    localPos.x = content.localPosition.x; // x 위치는 유지
                    content.localPosition = localPos; // content 위치 업데이트
                }
            }
            // 아래로 스크롤 중일 때
            else if (velocity.y < 0)
            {
                // 뷰포트 하단 밖으로 벗어난 아이템들을 찾음
                var targets = m_contents2.Where(t =>
                {
                    bool result = t.offsetMax.y < content.InverseTransformPoint(viewport.position).y + viewport.rect.yMin - m_buffer;
                    return result;
                });

                bool isUpdated = false;

                // 이전 인덱스들이 유효한지 확인
                for (int i = m_indexMin - 1; i >= m_indexMin - targets.Count(); i--)
                {
                    isUpdated = onVerifyIndex?.Invoke(i) ?? true;
                    if (isUpdated)
                    {
                        break;
                    }
                }

                if (isUpdated)
                {
                    // 최소/최대 인덱스 업데이트
                    m_indexMin -= targets.Count();
                    m_indexMax -= targets.Count();

                    // 모든 아이템의 데이터 업데이트
                    for (int targetIndex = 0, dataIndex = m_indexMin; targetIndex < m_contents2.Length; targetIndex++, dataIndex++)
                    {
                        RectTransform target = m_contents2[targetIndex];

                        // 자동 비활성화 모드에 따라 활성화 여부 결정 및 업데이트 이벤트 실행
                        target.gameObject.SetActive(!m_autoInactive || (onVerifyIndex?.Invoke(dataIndex) ?? true));

                        ExecuteEvents.Execute<IContent>(target.gameObject, null, (handler, data) =>
                        {
                            handler.Update(dataIndex);
                        });
                    }

                    // 스크롤 위치 보정
                    RectTransform child = (RectTransform)content.GetChild(content.childCount - targets.Count() - 1); // 마지막에서 targets.Count() 만큼 떨어진 자식
                    Vector2 childWorldButtomPos = content.TransformPoint(child.offsetMin);
                    Vector3 localPos = content.parent.InverseTransformPoint(childWorldButtomPos);

                    if (m_isDrag)
                    {
                        m_delta += content.offsetMin - (Vector2)localPos;
                    }

                    localPos.x = content.localPosition.x; // x 위치는 유지
                    localPos.y += content.rect.height; // content 높이만큼 이동 (아래로 스크롤 시)

                    content.localPosition = localPos; // content 위치 업데이트
                }
            }
        }

        // 가로 루프 모드 업데이트
        private void UpdateHorizontalLoopMode()
        {
            if (!horizontal) // 가로 스크롤이 아니면 리턴
            {
                return;
            }

            // 왼쪽으로 스크롤 중일 때
            if (velocity.x < 0)
            {
                // 뷰포트 좌측 밖으로 벗어난 아이템들을 찾음
                var targets = m_contents.Where(t =>
                {
                    bool result = t.Key.offsetMax.x < content.InverseTransformPoint(viewport.position).x + viewport.rect.xMin - m_buffer;
                    return result;
                }).OrderBy(t => t.Value); // 인덱스 순으로 정렬

                bool isUpdated = targets.Any(t =>
                {
                    // 다음 인덱스가 유효한지 확인
                    var result = onVerifyIndex?.Invoke(t.Value + content.childCount) ?? true;
                    return result;
                });

                if (isUpdated)
                {
                    // 현재 아이템 중 가장 큰 인덱스
                    int lastIndex = m_contents.Max(t => t.Value);
                    foreach (var target in targets)
                    {
                        // 아이템의 인덱스를 다음 인덱스로 업데이트
                        m_contents[target.Key] = ++lastIndex;
                        // 아이템을 맨 마지막 자식으로 이동
                        target.Key.SetAsLastSibling();

                        // 자동 비활성화 모드에 따라 처리
                        if (m_autoInactive)
                        {
                            m_autoInactives.Add(target.Key);
                        }
                        else
                        {
                            target.Key.gameObject.SetActive(true);

                            // IContent 인터페이스를 구현한 컴포넌트에 업데이트 이벤트 실행
                            ExecuteEvents.Execute<IContent>(target.Key.gameObject, null, (handler, data) =>
                            {
                                handler.Update(lastIndex);
                            });
                        }
                    }

                    // 스크롤 위치 보정
                    RectTransform child = (RectTransform)content.GetChild(0); // 첫 번째 자식
                    Vector2 childWorldLeftPos = content.TransformPoint(child.offsetMin);
                    Vector3 localPos = content.parent.InverseTransformPoint(childWorldLeftPos);

                    if (m_isDrag)
                    {
                        m_delta += content.offsetMin - (Vector2)localPos;
                    }

                    localPos.y = content.localPosition.y; // y 위치는 유지
                    content.localPosition = localPos; // content 위치 업데이트
                }
            }
            // 오른쪽으로 스크롤 중일 때
            else if (velocity.x > 0)
            {
                // 뷰포트 우측 밖으로 벗어난 아이템들을 찾음
                var targets = m_contents.Where(t =>
                {
                    bool result = t.Key.offsetMin.x > content.InverseTransformPoint(viewport.position).x + viewport.rect.xMax + m_buffer;
                    return result;
                }).OrderByDescending(t => t.Value); // 인덱스 역순으로 정렬

                bool isUpdated = targets.Any(t =>
                {
                    // 이전 인덱스가 유효한지 확인
                    var result = onVerifyIndex?.Invoke(t.Value - content.childCount) ?? true;
                    return result;
                });

                if (isUpdated)
                {
                    // 현재 아이템 중 가장 작은 인덱스
                    int firstIndex = m_contents.Min(t => t.Value);
                    foreach (var target in targets)
                    {
                        // 아이템의 인덱스를 이전 인덱스로 업데이트
                        m_contents[target.Key] = --firstIndex;
                        // 아이템을 맨 앞 자식으로 이동
                        target.Key.SetAsFirstSibling();

                        // 자동 비활성화 모드에 따라 처리
                        if (m_autoInactive)
                        {
                            m_autoInactives.Add(target.Key);
                        }
                        else
                        {
                            target.Key.gameObject.SetActive(true);

                            // IContent 인터페이스를 구현한 컴포넌트에 업데이트 이벤트 실행
                            ExecuteEvents.Execute<IContent>(target.Key.gameObject, null, (handler, data) =>
                            {
                                handler.Update(firstIndex);
                            });
                        }
                    }

                    // 스크롤 위치 보정
                    RectTransform child = (RectTransform)content.GetChild(content.childCount - 1); // 마지막 자식
                    Vector2 childWorldRightPos = content.TransformPoint(child.offsetMax);
                    Vector3 localPos = content.parent.InverseTransformPoint(childWorldRightPos);

                    if (m_isDrag)
                    {
                        m_delta += content.offsetMax - (Vector2)localPos;
                    }

                    localPos.y = content.localPosition.y; // y 위치는 유지
                    localPos.x -= content.rect.width; // content 너비만큼 이동 (오른쪽으로 스크롤 시)

                    content.localPosition = localPos; // content 위치 업데이트
                }
            }
        }

        // (미구현) LayoutGroup이 GridLayoutGroup일 때 SetAsFirstSibiling 로직
        private void SetAsFirstSibiling(RectTransform transform)
        {
            if (m_layoutGroup is GridLayoutGroup)
            {
                var grid = m_layoutGroup as GridLayoutGroup;

                if (horizontal)
                {
                    if (grid.startAxis == GridLayoutGroup.Axis.Horizontal &&
                        (grid.startCorner == GridLayoutGroup.Corner.LowerRight || grid.startCorner == GridLayoutGroup.Corner.UpperRight))
                    {
                        var last = m_contents.Where(t => t.Key.position.y == transform.position.y).OrderByDescending(t => t.Key.position.x).FirstOrDefault();
                    }
                    else
                    {
                    }
                }
            }
        }
    }
}