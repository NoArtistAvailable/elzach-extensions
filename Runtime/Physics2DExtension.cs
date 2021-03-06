﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace elZach.Common
{
    public static class Physics2DExtension
    {
        public static bool OverlapBox(Vector2 point, Vector2 size, Collider2D ignore)
        {
            ContactFilter2D filter = new ContactFilter2D();
            List<Collider2D> hits = new List<Collider2D>();
            if (Physics2D.OverlapArea(point - size * 0.5f, point + size * 0.5f, filter, hits) > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit != ignore)
                        return true;
                }
                return false;
            }
            else return false;
        }

        public static bool OverlapBox(Vector2 point, Vector2 size, Collider2D ignore, LayerMask mask)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(mask);
            List<Collider2D> hits = new List<Collider2D>();
            if (Physics2D.OverlapArea(point - size * 0.5f, point + size * 0.5f, filter, hits) > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit != ignore)
                        return true;
                }
                return false;
            }
            else return false;
        }

        public static bool RayCast(Vector2 pos, Vector2 dir, List<Collider2D> ignore, ref RaycastHit2D result)
        {
            ContactFilter2D filter = new ContactFilter2D();
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            if (Physics2D.Raycast(pos, dir, filter, hits) > 0)
            {
                foreach (var hit in hits)
                {
                    foreach (var ignoredCollider in ignore)
                    {
                        if (hit.collider == ignoredCollider)
                            break;
                    }
                    result = hit;
                    return hit;
                }
                return false;
            }
            return false;
        }

        public static bool RayCast(Vector2 pos, Vector2 dir, float maxDistance, List<Collider2D> ignore, ref RaycastHit2D result)
        {
            ContactFilter2D filter = new ContactFilter2D();
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            if (Physics2D.Raycast(pos, dir, filter, hits, maxDistance) > 0)
            {
                foreach (var hit in hits)
                {
                    foreach (var ignoredCollider in ignore)
                    {
                        if (hit.collider == ignoredCollider)
                            break;
                    }
                    result = hit;
                    return hit;
                }
                return false;
            }
            return false;
        }

        public static bool RayCast(Vector2 pos, Vector2 dir, Collider2D ignore, ref RaycastHit2D result)
        {
            ContactFilter2D filter = new ContactFilter2D();
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            if (Physics2D.Raycast(pos, dir, filter, hits) > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.collider == ignore)
                        continue;
                    result = hit;
                    return hit;
                }
                return false;
            }
            return false;
        }

        public static bool RayCast(Vector2 pos, Vector2 dir, float maxDistance ,Collider2D ignore, ref RaycastHit2D result)
        {
            ContactFilter2D filter = new ContactFilter2D();
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            if (Physics2D.Raycast(pos, dir, filter, hits, maxDistance) > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.collider == ignore)
                        continue;
                    result = hit;
                    return hit;
                }
                return false;
            }
            return false;
        }
    }
}
