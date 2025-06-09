import DropDownFilter from "@/components/ToolbarSearchFilter/ToolbarComponents/DropDownFilter";
import FromToDates from "@/components/ToolbarSearchFilter/ToolbarComponents/FromToDates";
import RangeFilter from "@/components/ToolbarSearchFilter/ToolbarComponents/RangeFilter";
import useApi from "@/hooks/useApi";
import type { EmployeesRequest } from "@/types/EmployeesRequest";
import type { FilterDefaultsResponse } from "@/types/FilterDefaultsResponse";
import { useEffect, useState } from "react";
import { Offcanvas, Button } from "react-bootstrap";
import "react-datepicker/dist/react-datepicker.css";

interface FiltersOffcanvasProps {
    show: boolean;
    onHide: () => void;
    setRequest: React.Dispatch<React.SetStateAction<EmployeesRequest>>
}

const defaultFilters = {
    regDateFrom: null as Date | null,
    regDateTo: null as Date | null,
    gender: "All",
    country: "All",
    salary: 200000,
    birthDateFrom: null as Date | null,
    birthDateTo: null as Date | null,
};

export default function FiltersOffcanvas(
    {
        show,
        onHide,
        setRequest
    }: FiltersOffcanvasProps) {
    const [filters, setFilters] = useState({ ...defaultFilters });
    const [countires, setCountries] = useState<string[]>([]);
    const [maxSalary, setMaxSalary] = useState<number>(200000);

    const GET_COUNTRIES = '/FilterDefaults/GetAllCountries';
    const GET_MAX_SALARY = '/FilterDefaults/GetMaxSalary';

    const { data: filtersDataCountries, fetchData: fetchCountries } = useApi<FilterDefaultsResponse>(GET_COUNTRIES,);

    const { data: filtersDataMaxSalary, fetchData: fetchMaxSalary } = useApi<FilterDefaultsResponse>(GET_MAX_SALARY,);

    useEffect(() => {
        fetchCountries();
        fetchMaxSalary();
    }, []);

    useEffect(() => {
        if (filtersDataCountries?.countries) {
            setCountries(filtersDataCountries.countries);
        }

        if (filtersDataMaxSalary?.maxSalary) {
            setMaxSalary(filtersDataMaxSalary.maxSalary);
        }
    }, [filtersDataCountries, filtersDataMaxSalary]);

    const handleClear = () => {
        setFilters({ ...defaultFilters });
        setRequest(prev => ({
            ...prev,
            RegistrationDateFrom: undefined,
            RegistrationDateTo: undefined,
            Gender: undefined,
            Country: undefined,
            Salary: undefined,
            BirthDateFrom: undefined,
            BirthDateTo: undefined,
            Page: 1
        }));
        onHide();
    };

    const handleApplyFilter = () => {
        setRequest(prev => ({
            ...prev,
            RegistrationDateFrom: filters.regDateFrom ?? undefined,
            RegistrationDateTo: filters.regDateTo ?? undefined,
            Gender: filters.gender && filters.gender !== "All" ? filters.gender : undefined,
            Country: filters.country && filters.country !== "All" ? filters.country : undefined,
            Salary: filters.salary ? filters.salary : undefined,
            BirthDateFrom: filters.birthDateFrom ?? undefined,
            BirthDateTo: filters.birthDateTo ?? undefined,
            Page: 1
        }));
        onHide();
    };

    return (
        <Offcanvas show={show} onHide={onHide} scroll={false} placement="end" style={{ zIndex: 2000 }}>
            <Offcanvas.Header closeButton>
                <Offcanvas.Title>Filters</Offcanvas.Title>
            </Offcanvas.Header>
            <Offcanvas.Body className="p-0 d-flex flex-column" style={{ height: "100%" }}>

                <div className="flex-grow-1 overflow-auto p-3" >

                    <FromToDates
                        label="Registration Date"
                        fromDate={filters.regDateFrom}
                        toDate={filters.regDateTo}
                        setFrom={date => setFilters(f => ({ ...f, regDateFrom: date }))}
                        setTo={date => setFilters(f => ({ ...f, regDateTo: date }))}
                        dateFormat="dd/MM/yyyy"
                    />

                    <hr />

                    <DropDownFilter
                        label="Gender"
                        value={filters.gender}
                        options={["All", "Male", "Female"]}
                        onChange={val => setFilters(f => ({ ...f, gender: val }))}
                    />

                    <hr />

                    <DropDownFilter
                        label="Country"
                        value={filters.country}
                        options={["All", ...countires]}
                        onChange={val => setFilters(f => ({ ...f, country: val }))}
                    />

                    <hr />

                    <RangeFilter
                        label="Salary Range"
                        min={0}
                        max={maxSalary}
                        value={maxSalary}
                        onChange={range => setFilters(f => ({ ...f, salary: range }))}
                    />
                    <hr />

                    <FromToDates
                        label="Birth Date"
                        fromDate={filters.birthDateFrom}
                        toDate={filters.birthDateTo}
                        setFrom={date => setFilters(f => ({
                            ...f, birthDateFrom: date
                        }))}
                        setTo={date => setFilters(f => ({ ...f, birthDateTo: date }))}
                        dateFormat="dd/MM/yyyy"
                    />
                </div>

                <div className="border-top bg-white p-3 d-flex justify-content-between">
                    <Button variant="outline-secondary" onClick={handleClear}>
                        Clear Filters
                    </Button>
                    <Button variant="primary" onClick={handleApplyFilter}>
                        Apply Filter
                    </Button>
                </div>
            </Offcanvas.Body>
        </Offcanvas>
    );
}