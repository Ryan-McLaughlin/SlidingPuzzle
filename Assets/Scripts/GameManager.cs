using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections;

public class GameManager: MonoBehaviour
{
    [SerializeField] private Transform gameTransform;
    [SerializeField] private Transform piecePrefab;

    private List<Transform> pieces;
    private int emptyLocation;
    private bool shuffling = false;

    public int size;

    void Start()
    {
        pieces = new List<Transform>();
        CreateGamePieces(0.01f);
    }

    void Update()
    {
        // TODO: move this from Update() to when a piece is moved!
        if(!shuffling && CheckCompletion())
        {
            shuffling = true;
            StartCoroutine(WaitShuffle(0.5f));
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        //Debug.Log("GameManager.OnClick()");
        if(context.started)
        {
            // Debug.Log("context.started");
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePosition), Vector2.zero);
            if(hit)
            {
                Debug.Log($"pieces.Count: {pieces.Count}");
                for(int i = 0; i < pieces.Count; i++)
                {
                    // Use a slightly different hit check to see if the piece was clicked.
                    if(hit.transform == pieces[i])
                    {
                        if(SwapIfValid(i, -size, size)) { break; }
                        if(SwapIfValid(i, +size, size)) { break; }
                        if(SwapIfValid(i, -1, 0)) { break; }
                        if(SwapIfValid(i, +1, size - 1)) { break; }
                    }
                }
            }
        }
    }

    private void CreateGamePieces(float gapThickness)
    {
        float width = 1 / (float)size;

        for(int row = 0; row < size; row++)
        {
            for(int col = 0; col < size; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameTransform);

                pieces.Add(piece);

                piece.localPosition = new Vector3(-1 + (2 * width * col) + width,
                                                  +1 - (2 * width * row) + width,
                                                  0);
                piece.localScale = ((2 * width) - gapThickness) * Vector3.one;
                piece.name = $"{(row * size) + col}";

                if((row == size - 1) && (col == size - 1))
                {
                    emptyLocation = (size * size) - 1;
                    piece.gameObject.SetActive(false);
                }
                else
                {
                    float gap = gapThickness / 2;
                    Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                    Vector2[] uv = new Vector2[4];

                    uv[0] = new Vector2((width * col) + gap, 1 - ((width * (row + 1)) - gap));
                    uv[1] = new Vector2((width * (col + 1)) - gap, 1 - ((width * (row + 1)) - gap));
                    uv[2] = new Vector2((width * col) + gap, 1 - ((width * row) + gap));
                    uv[3] = new Vector2((width * (col + 1)) - gap, 1 - ((width * row) + gap));

                    mesh.uv = uv;
                }
            }
        }
    }

    private bool SwapIfValid(int i, int offset, int colCheck)
    {
        Debug.Log("GameManager.SwapIfValid");
        if(((i % size) != colCheck) && ((i + offset) == emptyLocation))
        {
            (pieces[i], pieces[i + offset]) = (pieces[i + offset], pieces[i]);
            (pieces[i].localPosition, pieces[i + offset].localPosition) = ((pieces[i + offset].localPosition, pieces[i].localPosition));
            emptyLocation = i;
            return true;
        }

        return false;
    }

    private bool CheckCompletion()
    {
        for(int i = 0; i < pieces.Count; i++)
        {
            if(pieces[i].name != $"{i}")
            {
                return false;
            }
        }

        return true;
    }

    private IEnumerator WaitShuffle(float duration)
    {
        yield return new WaitForSeconds(duration);
        Shuffle();
        shuffling = false;
    }

    // Brute force shuffling
    private void Shuffle()
    {
        int count = 0;
        int last = 0;

        while(count < (size * size * size))
        {
            int rnd = Random.Range(0, size * size);

            if(rnd == last) { continue; }
            last = emptyLocation;

            if(SwapIfValid(rnd, -size, size))
            {
                count++;
            }
            else if(SwapIfValid(rnd, +size, size))
            {
                count++;
            }
            else if(SwapIfValid(rnd, -1, 0))
            {
                count++;
            }
            else if(SwapIfValid(rnd, +1, size - 1))
            {
                count++;
            }
        }
    }

}
