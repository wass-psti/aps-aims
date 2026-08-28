import { useEffect, useMemo, useState } from "react";
import { aimsApi } from "../lib/api";
import type {
  AssetCategory,
  AssetLocation,
  Branch,
  Company,
  Department,
} from "../types/aims";

interface MasterDataState {
  companies: Company[];
  categories: AssetCategory[];
  branches: Branch[];
  departments: Department[];
  locations: AssetLocation[];
  loading: boolean;
  error: string | null;
}

const activeOnly = <T extends { isActive: boolean }>(records: T[]) =>
  records.filter((record) => record.isActive);

export function useAssetMasterData(
  companyId: string,
  branchId: string,
) {
  const [state, setState] = useState<MasterDataState>({
    companies: [],
    categories: [],
    branches: [],
    departments: [],
    locations: [],
    loading: true,
    error: null,
  });

  useEffect(() => {
    let cancelled = false;

    async function loadInitialData() {
      try {
        const [companies, categories] = await Promise.all([
          aimsApi.getCompanies(),
          aimsApi.getCategories(),
        ]);

        if (cancelled) return;

        setState((current) => ({
          ...current,
          companies: activeOnly(companies),
          categories: activeOnly(categories),
          loading: false,
          error: null,
        }));
      } catch (error) {
        if (cancelled) return;

        setState((current) => ({
          ...current,
          loading: false,
          error:
            error instanceof Error
              ? error.message
              : "Unable to load master data.",
        }));
      }
    }

    loadInitialData();

    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    let cancelled = false;

    async function loadBranches() {
      if (!companyId) {
        setState((current) => ({
          ...current,
          branches: [],
          departments: [],
          locations: [],
        }));
        return;
      }

      try {
        const branches = await aimsApi.getBranchesByCompany(companyId);

        if (cancelled) return;

        setState((current) => ({
          ...current,
          branches: activeOnly(branches),
          departments: [],
          locations: [],
          error: null,
        }));
      } catch (error) {
        if (cancelled) return;

        setState((current) => ({
          ...current,
          branches: [],
          departments: [],
          locations: [],
          error:
            error instanceof Error
              ? error.message
              : "Unable to load branches.",
        }));
      }
    }

    loadBranches();

    return () => {
      cancelled = true;
    };
  }, [companyId]);

  useEffect(() => {
    let cancelled = false;

    async function loadBranchData() {
      if (!branchId) {
        setState((current) => ({
          ...current,
          departments: [],
          locations: [],
        }));
        return;
      }

      try {
        const [departments, locations] = await Promise.all([
          aimsApi.getDepartmentsByBranch(branchId),
          aimsApi.getLocationsByBranch(branchId),
        ]);

        if (cancelled) return;

        setState((current) => ({
          ...current,
          departments: activeOnly(departments),
          locations: activeOnly(locations),
          error: null,
        }));
      } catch (error) {
        if (cancelled) return;

        setState((current) => ({
          ...current,
          departments: [],
          locations: [],
          error:
            error instanceof Error
              ? error.message
              : "Unable to load branch master data.",
        }));
      }
    }

    loadBranchData();

    return () => {
      cancelled = true;
    };
  }, [branchId]);

  const categoryOptions = useMemo(
    () =>
      [...state.categories].sort((left, right) =>
        left.name.localeCompare(right.name),
      ),
    [state.categories],
  );

  return {
    ...state,
    categories: categoryOptions,
  };
}
