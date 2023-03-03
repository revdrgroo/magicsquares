using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareBuilder : MonoBehaviour {
  struct Piece {
    public GameObject piece;
    public Vector3 start;
    public Vector3 end;
    public bool animated;
  }

  [SerializeField]
  float Scale;
  [SerializeField]
  GameObject PiecePrefab;
  [SerializeField]
  int Size;
  [SerializeField]
  float Delay = 2.0f;
  [SerializeField]
  float Duration = 2.0f;
  [SerializeField]
  AnimationCurve AnimationAltitude;

  private int _outerSize;
  private Vector3 _offset;
  private Dictionary<int, Piece> _pieces = new Dictionary<int, Piece>();
  private Camera _camera;

  // Start is called before the first frame update
  void Start() {
    _camera = Camera.main;
    _camera.transform.Translate(-Vector3.forward * Scale * 2 * (Size - 3), Space.Self);
    _outerSize = Size * 2 - 1;
    float ofsVal = Scale * (-_outerSize / 2.0f + 0.5f);
    Vector3 _offset = new Vector3(ofsVal, 0, ofsVal);
    int outerOfs = (Size - 1) / 2;
    Vector2 start = new Vector2(Size - 1, _outerSize - 1);
    for (int x = 0; x < Size; x++) {
      for (int y = 0; y < Size; y++) {
        Vector2 pos = new Vector2(start.x - y + x - outerOfs, start.y - x - y - outerOfs);
        Vector3 pos3D = new Vector3((pos.x + outerOfs) * Scale, 0, (pos.y + outerOfs) * Scale) + _offset;
        GameObject go = Instantiate(PiecePrefab);
        TMPro.TMP_Text text = go.GetComponentInChildren<TMPro.TMP_Text>();
        int index = x * Size + y + 1;
        Vector3 translation = Vector3.zero;
        bool animated = true;
        if (pos.x < 0) {
          translation = new Vector3(Size * Scale, 0, 0);
        } else if (pos.x >= Size) {
          translation = new Vector3(-Size * Scale, 0, 0);
        } else if (pos.y < 0) {
          translation = new Vector3(0, 0, Size * Scale);
        } else if (pos.y >= Size) {
          translation = new Vector3(0, 0, -Size * Scale);
        } else {
          animated = false;
        }
        _pieces[index] = new Piece {
          piece = go,
          start = pos3D,
          end = pos3D + translation,
          animated = animated
        };
        text.text = (x * Size + y + 1).ToString();
        go.transform.position = pos3D;
      }
    }
    ShuffleAnimate();
  }

  private void ShuffleAnimate() {
    List<int> indices = new List<int>();
    for (int i = 1; i <= Size * Size; i++) {
      if (_pieces[i].animated) {
        indices.Add(i); 
      }
    }
    StartCoroutine(MovePieces_CO(Shuffle(indices.ToArray())));
  }

  private int[] Shuffle(int[] indices) {
    for (int i = indices.Length - 1; i > 0; i--) {
      int pos = UnityEngine.Random.Range(0, i + 1);
      int t = indices[i];
      indices[i] = indices[pos];
      indices[pos] = t;
    }
    return indices;
  }

  private IEnumerator MovePieces_CO(int[] indices) {
    yield return new WaitForSeconds(Delay);
    foreach (int i in indices) {
      Piece piece = _pieces[i];
      if (!piece.animated) {
        continue;
      }
      float ellapsed = 0;
      while (ellapsed < Duration) {
        float parameter = Mathf.Clamp01(ellapsed / Duration);
        Vector3 pos = Vector3.Lerp(piece.start, piece.end, parameter);
        pos.y = AnimationAltitude.Evaluate(parameter) * Scale;
        piece.piece.transform.position = pos;
        ellapsed += Time.deltaTime;
        yield return new WaitForEndOfFrame();
      }
      piece.piece.transform.position = piece.end;
    }
  }
}
