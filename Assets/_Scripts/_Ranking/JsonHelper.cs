using UnityEngine;
using System;

/// <summary>
/// Unity의 내장 JsonUtility가 최상위 배열 형태의 JSON을 직접 파싱하지 못하는 한계를 극복하기 위한 헬퍼 함수
/// JSON 배열을 임시 객체로 감싸서 반환
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// JSON 배열 문자열을 C# 배열 객체로 변환
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    public static T[] FromJson<T>(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return new T[0];
        }

        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    /// <summary>
    /// C# 배열 객체를 JSON 문자열로 반환
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="prettyPrint"></param>
    /// <returns></returns>
    public static string ToJson<T>(T[] array, bool prettyPrint = false)
    {
        if (array == null)
        {
            return "{\"array\":[]}";
        }

        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.array = array;

        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    /// <summary>
    /// 배열을 JSON 객체 형태로 감싸기 위한 직렬화용 내부 Wrapper 클래스
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}