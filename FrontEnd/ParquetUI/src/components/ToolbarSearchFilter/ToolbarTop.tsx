import FunnelFillIcon from "@/assets/FunnelFillIcon";
import GearIcon from "@/assets/GearIcon";
import FiltersOffcanvas from "@/components/ToolbarSearchFilter/ToolbarComponents/FiltersOffcanvas";
import OptionsOffcanvas from "@/components/ToolbarSearchFilter/ToolbarComponents/OptionsOffcanvas";
import type { EmployeesRequest } from "@/types/EmployeesRequest";
import type { SearchOptionFields } from "@/types/SearchOptionFields";
import { useState } from "react";
import { Button, Form, InputGroup, Stack } from "react-bootstrap";

export default function ToolbarTop(
    {
        setRequest
    }: {
        setRequest: React.Dispatch<React.SetStateAction<EmployeesRequest>>
    }) {
    const [showOptions, setShowOptions] = useState(false);
    const [showFilters, setShowFilters] = useState(false);
    const [searchTerm, setSerchTerm] = useState<string>("");
    const [searchOptionFields, setSearchOptionFields] = useState<SearchOptionFields>({
        searchFirstName: true,
        searchLastName: true,
        searchEmail: true,
        searchComments: true,
        searchTitle: true,
    });

    const handleSubmit = () => {
        console.log(searchOptionFields);
        setRequest(prev => ({
            ...prev,
            SearchTerm: searchTerm,
            searchFirstName: searchOptionFields.searchFirstName,
            searchLastName: searchOptionFields.searchLastName,
            searchEmail: searchOptionFields.searchEmail,
            searchComments: searchOptionFields.searchComments,
            searchTitle: searchOptionFields.searchTitle,
            Page: 1
        }));
    };

    return (
        <Stack
            gap={1}
            className="border border-3 shadow-sm rounded p-2 position-fixed top-0 start-0 end-0 bg-white"
            style={{ zIndex: 1100 }}
        >
            <Stack direction="horizontal" gap={2}>
                <Button
                    variant="outline-primary"
                    onClick={() => setShowOptions(true)}
                >
                    <GearIcon />
                </Button>

                
                <InputGroup className="me-auto">
                    <Form.Control
                        placeholder="Search term..."
                        value={searchTerm ?? ""}
                        onChange={e => setSerchTerm(e.target.value)}
                    />
                    {searchTerm && (
                        <Button
                            variant="outline-secondary"
                            size="sm"
                            style={{
                                position: "absolute",
                                right: 10,
                                top: "50%",
                                transform: "translateY(-50%)",
                                zIndex: 2,
                                padding: "0.25rem 0.5rem"
                            }}
                            tabIndex={-1}
                            onClick={() => {
                                setSerchTerm("");
                                setRequest(prev => ({
                                    ...prev,
                                    SearchTerm: "",
                                    Page: 1
                                }));
                            }}
                        >
                            <span aria-hidden="true">&times;</span>
                        </Button>
                    )}
                </InputGroup>

                <Button
                    variant="primary"
                    onClick={handleSubmit}
                >
                    Submit
                </Button>

                <div className="vr" />

                <Button
                    variant="secondary"
                    onClick={() => setShowFilters(true)}
                >
                    <FunnelFillIcon />
                </Button>
            </Stack>

            <OptionsOffcanvas
                show={showOptions}
                onHide={() => setShowOptions(false)}
                searchOptionFields={searchOptionFields}
                setSearchFields={setSearchOptionFields}
            />

            <FiltersOffcanvas
                show={showFilters}
                onHide={() => setShowFilters(false)}
                setRequest={setRequest}
            />
        </Stack>
    );
}